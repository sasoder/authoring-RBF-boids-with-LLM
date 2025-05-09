from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import StreamingResponse
from faster_whisper import WhisperModel
import os
import logging
import asyncio
from dotenv import load_dotenv
from pydantic import BaseModel
from api.generate import router as generate_router
load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

app = FastAPI()

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


app.include_router(generate_router)

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