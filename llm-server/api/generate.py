import os
import logging
import ollama
from fastapi import APIRouter, HTTPException
from dotenv import load_dotenv
from utils.prompts import construct_prompt
from utils.types import GenerateRequest, GenerateResponse
import json

router = APIRouter()

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

ollama_host = os.getenv("OLLAMA_HOST", "http://localhost:11434")
logger.info(f"Configuring Ollama client for host: {ollama_host}")
ollama_model_name = os.getenv("OLLAMA_MODEL", "llama3")
ollama_client = ollama.Client(host=ollama_host)

@router.post("/generate", response_model=GenerateResponse)
async def generate_text(request_data: GenerateRequest) -> GenerateResponse:
    logger.info(f"Received generation request with prompt: '{request_data.prompt[:50]}...' using configured model: {ollama_model_name}")
    try:
        ollama_response = ollama_client.generate(
            model=ollama_model_name,
            prompt=construct_prompt(request_data),
            options={
                "temperature": request_data.temperature,
                "top_k": request_data.top_k,
                "top_p": request_data.top_p,
            },
            format=GenerateResponse.model_json_schema(),
            raw=True,
            stream=False,
        )
        
        raw_json_string = ollama_response.get('response')
        logger.info(f"Raw response from Ollama: {raw_json_string}")

        if not raw_json_string:
            logger.error("Ollama returned an empty response string.")
            raise HTTPException(status_code=500, detail="Ollama returned an empty response.")

        try:
            # Parse the JSON string into the GenerateResponse Pydantic model
            parsed_response = GenerateResponse.model_validate_json(raw_json_string)
            return parsed_response
        except json.JSONDecodeError as e:
            logger.error(f"Failed to decode JSON response from Ollama: {e}. Response was: {raw_json_string}", exc_info=True)
            raise HTTPException(status_code=500, detail=f"Failed to decode JSON from Ollama: {str(e)}")
        except Exception as e:
            logger.error(f"Failed to validate Ollama response against Pydantic model: {e}. Response was: {raw_json_string}", exc_info=True)
            raise HTTPException(status_code=500, detail=f"Invalid data structure from Ollama: {str(e)}")

    except ollama.ResponseError as e:
        logger.error(f"Ollama API responded with an error: {e.status_code} - {e.error}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Ollama API error: {e.error}")
    except Exception as e:
        logger.error(f"Error during Ollama generation: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Ollama generation failed: {str(e)}")