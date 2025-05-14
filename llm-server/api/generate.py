import os
import logging
import json
from fastapi import APIRouter, HTTPException
from dotenv import load_dotenv

from openai import OpenAI, APIError as OpenAI_APIError
import ollama
from ollama import ResponseError as Ollama_ResponseError

from utils.prompts import (
    system_prompt,
    construct_main_prompt_content,
    construct_ollama_prompt
)
from utils.types import GenerateRequest, GenerateResponse

router = APIRouter()
load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# --- OpenAI Client Configuration ---
openai_api_key = os.getenv("OPENAI_API_KEY")
if not openai_api_key:
    logger.warning("OPENAI_API_KEY environment variable not found. OpenAI calls will fail.")
openai_client = OpenAI(api_key=openai_api_key)
DEFAULT_OPENAI_MODEL = "gpt-4o-mini"


# --- Ollama Client Configuration ---
ollama_host = os.getenv("OLLAMA_HOST", "http://localhost:11434")
try:
    ollama_client = ollama.Client(host=ollama_host)
    ollama_client.list() 
    logger.info(f"Successfully connected to Ollama host: {ollama_host}")
except Exception as e:
    logger.warning(f"Could not connect to Ollama host: {ollama_host}. Ollama calls will fail. Error: {e}")
    ollama_client = None

DEFAULT_OLLAMA_MODEL = os.getenv("OLLAMA_MODEL", "llama3:8b-instruct-q4_K_M")


async def _generate_with_openai(request_data: GenerateRequest, model_to_use: str) -> GenerateResponse:
    logger.info(f"Generating with OpenAI model: '{model_to_use}'")
    
    main_content = construct_main_prompt_content(request_data)
    messages = [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": main_content}
    ]

    try:
        completion_result = openai_client.beta.chat.completions.parse(
            model=model_to_use,
            messages=messages,
            response_format=GenerateResponse,
            temperature=request_data.temperature,
            top_p=request_data.top_p,
        )
        
        if not completion_result.choices or not completion_result.choices[0].message:
            logger.error("OpenAI response did not contain the expected choices or message structure.")
            raise HTTPException(status_code=500, detail="Invalid response structure from OpenAI.")

        message_data = completion_result.choices[0].message
        parsed_response = message_data.parsed 

        if not isinstance(parsed_response, GenerateResponse):
            raw_content_for_log = message_data.content if hasattr(message_data, 'content') else "No raw content available"
            logger.error(f"OpenAI .parse() method returned unexpected type: {type(parsed_response)}. Expected GenerateResponse. Raw content: {raw_content_for_log}")
            raise HTTPException(status_code=500, detail="Unexpected data structure from OpenAI after parsing.")
        
        logger.info(f"Successfully parsed response from OpenAI: {parsed_response.model_dump_json(indent=2)}")
        return parsed_response

    except OpenAI_APIError as e: 
        logger.error(f"OpenAI API responded with an error: Status {e.status_code} - {e.message}", exc_info=True)
        request_id = getattr(e, 'request_id', None)
        detail_msg = f"OpenAI API error: {e.message}"
        if request_id:
            detail_msg += f" (Request ID: {request_id})"
        status_code = e.status_code if isinstance(e.status_code, int) else 500
        raise HTTPException(status_code=status_code, detail=detail_msg)
    except Exception as e: 
        logger.error(f"Error during OpenAI generation or parsing: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"OpenAI generation/parsing failed: {str(e)}")


async def _generate_with_ollama(request_data: GenerateRequest, model_to_use: str) -> GenerateResponse:
    logger.info(f"Generating with Ollama model: '{model_to_use}'")
    if not ollama_client:
        logger.error("Ollama client not available. Cannot generate with Ollama.")
        raise HTTPException(status_code=503, detail="Ollama service is not available.")

    full_prompt_for_ollama = construct_ollama_prompt(request_data)
    
    try:
        ollama_response_data = ollama_client.generate(
            model=model_to_use,
            prompt=full_prompt_for_ollama,
            options={
                "temperature": request_data.temperature,
                "top_k": request_data.top_k,
                "top_p": request_data.top_p,
            },
            format=GenerateResponse.model_json_schema(),
            raw=True,
            stream=False,
        )
        
        raw_json_string = ollama_response_data.get('response')
        logger.info(f"Raw response from Ollama: {raw_json_string}")

        if not raw_json_string:
            logger.error("Ollama returned an empty response string.")
            raise HTTPException(status_code=500, detail="Ollama returned an empty response.")

        try:
            parsed_response = GenerateResponse.model_validate_json(raw_json_string)
            logger.info(f"Successfully parsed response from Ollama: {parsed_response.model_dump_json(indent=2)}")
            return parsed_response
        except json.JSONDecodeError as e:
            logger.error(f"Failed to decode JSON response from Ollama: {e}. Response was: {raw_json_string}", exc_info=True)
            raise HTTPException(status_code=500, detail=f"Failed to decode JSON from Ollama: {str(e)}")
        except Exception as e: # Catches Pydantic validation errors
            logger.error(f"Failed to validate Ollama response against Pydantic model: {e}. Response was: {raw_json_string}", exc_info=True)
            raise HTTPException(status_code=500, detail=f"Invalid data structure from Ollama: {str(e)}")

    except Ollama_ResponseError as e:
        logger.error(f"Ollama API responded with an error: {e.status_code} - {e.error}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Ollama API error: {e.error}")
    except Exception as e:
        logger.error(f"Error during Ollama generation: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"Ollama generation failed: {str(e)}")


@router.post("/generate", response_model=GenerateResponse)
async def generate_text(request_data: GenerateRequest) -> GenerateResponse:
    requested_model_name = request_data.model_id
    logger.info(f"Received generation request. User specified model: '{requested_model_name if requested_model_name else 'None (will use default Ollama)'}'. Prompt: '{request_data.prompt[:50]}...'")

    if requested_model_name and requested_model_name != "ollama":
        logger.info(f"Using OpenAI provider for model: {requested_model_name}")
        return await _generate_with_openai(request_data, requested_model_name)
    else:
        # No model specified by user, default to Ollama with its default model
        logger.info(f"No specific model requested, defaulting to Ollama with model: {DEFAULT_OLLAMA_MODEL}")
        return await _generate_with_ollama(request_data, DEFAULT_OLLAMA_MODEL)