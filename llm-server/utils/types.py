from pydantic import BaseModel

class Coordinate(BaseModel):
    x: float
    y: float
    z: float

class Vector(BaseModel):
    s: Coordinate
    e: Coordinate

class Bounds(BaseModel):
    min: Vector
    max: Vector

class WorldBounds(BaseModel):
    min: Coordinate
    max: Coordinate

class GameObject(BaseModel):
    name: str
    origin: Coordinate
    bounds: Bounds | None = None


class SceneGraph(BaseModel):
    world_bounds: WorldBounds
    game_objects: list[GameObject]


class GenerateRequest(BaseModel):
    prompt: str
    flock_position: Coordinate
    scene_graph: SceneGraph
    available_styles: list[str]
    model_id: str | None = None
    temperature: float | None = 0.5
    top_k: int | None = 10
    top_p: float | None = 0.6


class GenerateResponse(BaseModel):
    style: str
    vectors: list[Vector]