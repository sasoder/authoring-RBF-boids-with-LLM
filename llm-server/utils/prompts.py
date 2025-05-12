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
    - vectors: A list of directed line segments (vectors). Each segment is defined by a start coordinate `s` and an end coordinate `e`. These segments collectively define the flow field and should guide the boid flock.

    IMPORTANT CONSTRAINTS:
    1. All source points (start coordinates `s`) MUST have y=0 to match the scene's floor plane (XZ plane)
    2. The end points (end coordinates `e`) can have any y-coordinate to create the desired flow field
    3. The vectors should form a continuous path where the end point of one vector can serve as the start point of the next
    4. When creating patterns (like squares, circles, etc.), they should be drawn so they start on the XZ plane (floor) with y=0.

    PLEASE NOTE THAT UNITY'S COORDINATE SYSTEM IS LEFT-HANDED, MEANING THAT FROM THE PERSPECTIVE OF THE BOID FLOCK:
    - The X-AXIS is pointing to the RIGHT
    - The Y-AXIS is pointing UP
    - The Z-AXIS is pointing FORWARD

    For example, if the user's prompt is "circle the tower aggressively", and the tower position is (5, 0, 5), the world bounds are (0, 0, 0) to (10, 10, 10), the response should be:

    {{
        "style": "aggressive",
        "vectors": [
            {{
                "s": {{ "x": 3, "y": 0, "z": 3 }},
                "e": {{ "x": 7, "y": 2, "z": 3 }}
            }},
            {{
                "s": {{ "x": 7, "y": 0, "z": 3 }},
                "e": {{ "x": 7, "y": 2, "z": 7 }}
            }},
            {{
                "s": {{ "x": 7, "y": 0, "z": 7 }},
                "e": {{ "x": 3, "y": 2, "z": 7 }}
            }},
            {{
                "s": {{ "x": 3, "y": 0, "z": 7 }},
                "e": {{ "x": 3, "y": 2, "z": 3 }}
            }}
        ]
    }}

Note that these vectors define directed segments. They should be thought of as force vectors or path segments that guide the boid flock. In this example, they form a square path around the tower on the XZ plane (floor), with some vectors pointing upward to create a dynamic flow field. Generally, the end point `e` of one vector can serve as the start point `s` of the next to define a continuous path.'''


def construct_scene_graph_prompt(scene_graph: SceneGraph) -> str:
    return f"This is the corners of the bounding box of the scene (world_bounds) followed by the current objects available in the scene(game_objects): {repr(scene_graph)}"

def construct_available_styles_prompt(available_styles: list[str]) -> str:
    return f"The available styles are the following: {repr(available_styles)}"

def construct_flock_position_prompt(flock_position: Coordinate) -> str:
    x, y, z = flock_position
    return f"The current position of the boid flock is x={x}, y={y}, z={z}."

def construct_user_prompt(prompt: str) -> str:
    return f"The user's prompt is: {prompt}"
