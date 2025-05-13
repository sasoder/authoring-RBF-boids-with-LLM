import os
import logging
from openai import OpenAI, APIError
from fastapi import APIRouter, HTTPException
from dotenv import load_dotenv
from utils.prompts import system_prompt, construct_llm_input_content
from utils.types import GenerateRequest, GenerateResponse
import json

router = APIRouter()

load_dotenv()

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# Configure OpenAI client
openai_api_key = os.getenv("OPENAI_API_KEY")
if not openai_api_key:
    logger.warning("OPENAI_API_KEY environment variable not found. API calls will likely fail.")
openai_client = OpenAI(api_key=openai_api_key)
openai_model_name = "gpt-4.1-mini-2025-04-14"

@router.post("/generate", response_model=GenerateResponse)
async def generate_text(request_data: GenerateRequest) -> GenerateResponse:
    logger.info(f"Received generation request for OpenAI model: '{openai_model_name}' with prompt: '{request_data.prompt[:50]}...' using Pydantic model for structured output.")
    
    llm_input = construct_llm_input_content(request_data)
    messages = [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": llm_input}
    ]

    try:
        completion_result = openai_client.beta.chat.completions.parse(
            model=openai_model_name,
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

    except APIError as e: 
        logger.error(f"OpenAI API responded with an error: Status {e.status_code} - {e.message}", exc_info=True)
        request_id = getattr(e, 'request_id', None)
        detail_msg = f"OpenAI API error: {e.message}"
        if request_id:
            detail_msg += f" (Request ID: {request_id})"
        status_code = e.status_code if isinstance(e.status_code, int) else 500
        raise HTTPException(status_code=status_code, detail=detail_msg)
    except HTTPException:
        raise
    except Exception as e: 
        logger.error(f"Error during OpenAI generation or parsing: {e}", exc_info=True)
        raise HTTPException(status_code=500, detail=f"OpenAI generation/parsing failed: {str(e)}")