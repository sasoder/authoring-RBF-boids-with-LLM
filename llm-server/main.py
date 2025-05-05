from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import StreamingResponse
from faster_whisper import WhisperModel
import ollama
import os
import logging
import asyncio
from dotenv import load_dotenv
from pydantic import BaseModel

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI()

# --- Pydantic Models ---
class GenerateRequest(BaseModel):
    prompt: str

# Downloads model on first run if not found.
model_size = os.getenv("WHISPER_MODEL", "base.en")
compute_type = os.getenv("WHISPER_COMPUTE_TYPE", "int8")
device = os.getenv("WHISPER_DEVICE", "cpu")

try:
    logger.info(f"Loading Faster Whisper model: {model_size} ({compute_type} on {device})")
    whisper_model = WhisperModel(model_size, device=device, compute_type=compute_type)
    logger.info("Faster Whisper model loaded successfully.")
except Exception as e:
    logger.error(f"Error loading Faster Whisper model: {e}", exc_info=True)
    # Decide how to handle this - maybe exit or provide a dummy endpoint?
    # For now, we'll let FastAPI start but transcription will fail.
    whisper_model = None

# default ollama server runs at http://localhost:11434
# adjust OLLAMA_HOST env var if it's elsewhere
ollama_host = os.getenv("OLLAMA_HOST", "http://localhost:11434")
logger.info(f"Configuring Ollama client for host: {ollama_host}")
ollama_model_name = os.getenv("OLLAMA_MODEL", "llama3")
ollama_client = ollama.Client(host=ollama_host)

@app.get("/")
async def read_root():
    return {"message": "FastAPI server for LLM and Whisper is running."}

@app.post("/transcribe")
async def transcribe_audio(audio_file: UploadFile = File(...)):
    if whisper_model is None:
        logger.error("Whisper model not loaded. Cannot transcribe.")
        raise HTTPException(status_code=500, detail="Whisper model is not available.")

    logger.info(f"Received audio file: {audio_file.filename}, content type: {audio_file.content_type}")
    # save the uploaded file temporarily because faster-whisper needs a file path
    temp_file_path = f"/tmp/{audio_file.filename}"
    try:
        # read the file content asynchronously
        content = await audio_file.read()
        with open(temp_file_path, "wb") as f:
            f.write(content)
        logger.info(f"Saved temporary audio file to {temp_file_path}")

        # transcribe (this is blocking, run in thread pool)
        logger.info("Starting transcription...")
        # Run the potentially long-running transcription in a separate thread
        segments, info = await asyncio.to_thread(
            whisper_model.transcribe,
            temp_file_path,
            beam_size=5
        )
        logger.info(f"Transcription detected language '{info.language}' with probability {info.language_probability:.2f}")

        # Concatenate segments into a single transcript
        transcript = " ".join([segment.text for segment in segments])
        logger.info("Transcription finished successfully.")
        return {"transcript": transcript.strip()}

    except Exception as e:
        logger.error(f"Error during transcription: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Transcription failed: {str(e)}")
    finally:
        # Clean up the temporary file
        if os.path.exists(temp_file_path):
            os.remove(temp_file_path)
            logger.info(f"Removed temporary audio file: {temp_file_path}")
        await audio_file.close() # Ensure the file handle is closed

# --- Generator function for streaming ---
async def ollama_stream_generator(model_name: str, prompt: str):
    """Yields chunks from the Ollama stream."""
    try:
        # ollama.generate returns a generator when stream=True
        stream = ollama_client.generate(
            model=model_name,
            prompt=prompt,
            stream=True
        )
        for chunk in stream:
            if chunk and 'response' in chunk:
                 # Yield only the response part of the chunk
                 yield chunk['response']
    except Exception as e:
        logger.error(f"Error during Ollama streaming: {e}", exc_info=True)
        # Handle potential errors during streaming if necessary,
        # maybe yield an error message or log it.
        yield f"Error streaming response: {str(e)}" # Or raise an exception handled by FastAPI

@app.post("/generate")
async def generate_text(request_data: GenerateRequest):
    logger.info(f"Received generation request with prompt: '{request_data.prompt[:50]}...' using configured model: {ollama_model_name}")
    try:
        # Return a StreamingResponse, passing our async generator
        return StreamingResponse(
            ollama_stream_generator(ollama_model_name, request_data.prompt),
            media_type="text/plain" # Send as plain text chunks
        )
    except Exception as e:
        # This top-level exception handler might catch setup errors before streaming starts
        logger.error(f"Error setting up Ollama generation stream: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Ollama generation setup failed: {str(e)}") 