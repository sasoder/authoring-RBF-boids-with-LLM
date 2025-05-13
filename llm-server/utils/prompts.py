from utils.types import GenerateRequest, SceneGraph, Coordinate, GenerateResponse

system_prompt = """
You are a helpful assistant that generates a list of vectors that make up a flow field for a boid flock to follow. The user will provide you with a scene graph, a flock position, a list of available flying styles, and most importantly, a user prompt that describes the desired behavior of the boid flock. Based on the user's prompt, choose the best behavioral style from the available styles, and generate a list of vectors that make up a flow field for a boid flock to follow.
"""



def construct_prompt(request: GenerateRequest) -> str:
    scene_graph_str = construct_scene_graph_prompt(request.scene_graph)
    flock_position_str = construct_flock_position_prompt(request.flock_position)
    user_prompt_str = construct_user_prompt(request.prompt)
    available_styles_str = construct_available_styles_prompt(request.available_styles)
    return f'''{{system_prompt}}

    {scene_graph_str}

    {flock_position_str}

    {user_prompt_str}

    {available_styles_str}

    The response must be a JSON object with the following fields:
    - style: Based on the user's prompt, choose the best behavioral style from the available styles. The style must be one of the available styles.
    - vectors: A list of directed line segments (vectors). Each segment is defined by a start coordinate `s` and an end coordinate `e`. These segments collectively define the flow field and should guide the boid flock towards the desired destination or along the desired path.

    IMPORTANT CONSTRAINTS:
    1. The world bounds are the scene's world bounds and is defined by the min and max coordinates of the scene. The boid flock must not leave the scene's world bounds.
    2. The game_objects field is the list of game objects in the scene. The boid flock must not collide with the game objects in the scene.
    3. All source points (start coordinates `s`) and end points (end coordinates `e`) MUST be within the scene's world bounds.
    4. The vectors should be placed according to the positions of the game objects in the scene and the current position of the boid flock, to ensure that the boid flock can navigate the scene effectively and towards the desired destination or along the desired path without colliding with the game objects.
    5. There must be enough vectors to reliably guide the boid flock towards the desired destination or along the desired path. Some extra vectors can be added to ensure that the boid flock can navigate the scene effectively and towards the desired destination or along the desired path without colliding with the game objects.
    6. You must generate at least 6 vectors that are evenly spaced out across the scene, to ensure that the flow field generated is dense and reliable.
    7. The chosen style of the boid flock should be consistent with the user's prompt, even if the exact name of the style is not provided in the available styles the one that best matches the user's prompt should be chosen.

    PLEASE NOTE THAT UNITY'S COORDINATE SYSTEM IS LEFT-HANDED, MEANING THAT FROM THE PERSPECTIVE OF THE BOID FLOCK:
    - The X-AXIS is pointing to the RIGHT
    - The Y-AXIS is pointing UP
    - The Z-AXIS is pointing FORWARD

    For example, for a valid user prompt, the response should be:

    {{
        "style": "???",
        "vectors": [
            {{
                "s": {{ "x": ..., "y": ..., "z": ... }},
                "e": {{ "x": ..., "y": ..., "z": ... }}
            }},
            {{
                "s": {{ "x": ..., "y": ..., "z": ... }},
                "e": {{ "x": ..., "y": ..., "z": ... }}
            }},
            {{
                "s": {{ "x": ..., "y": ..., "z": ... }},
                "e": {{ "x": ..., "y": ..., "z": ... }}
            }},
            {{
                "s": {{ "x": ..., "y": ..., "z": ... }},
                "e": {{ "x": ..., "y": ..., "z": ... }}
            }}
        ]
    }}

Where "..." is a valid float value within the world bounds and "???" is a valid behavioural style from the available styles. Note that these vectors define directed segments. They should be thought of as force vectors or path segments that guide the boid flock.'''


def construct_scene_graph_prompt(scene_graph: SceneGraph) -> str:
    return f"This is the corners of the bounding box of the scene (world_bounds) followed by the current objects available in the scene(game_objects): {repr(scene_graph)}"

def construct_available_styles_prompt(available_styles: list[str]) -> str:
    return f"The available styles are the following: {repr(available_styles)}"

def construct_flock_position_prompt(flock_position: Coordinate) -> str:
    x, y, z = flock_position
    return f"The current position of the boid flock is x={x}, y={y}, z={z}."

def construct_user_prompt(prompt: str) -> str:
    return f"The user's prompt is: {prompt}"
