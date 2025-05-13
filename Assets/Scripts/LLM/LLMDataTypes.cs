using System.Collections.Generic;

[System.Serializable]
public class LLMCoordinate
{
    public float x;
    public float y;
    public float z;
}

[System.Serializable]
public class LLMVector
{
    public LLMCoordinate s;
    public LLMCoordinate e;

}

[System.Serializable]
public class LLMBoundsType
{
    public LLMVector min;
    public LLMVector max;
}

[System.Serializable]
public class LLMWorldBoundsType
{
    public LLMCoordinate min;
    public LLMCoordinate max;
}

[System.Serializable]
public class LLMGameObject
{
    public string name;
    public LLMCoordinate origin;
}

[System.Serializable]
public class LLMSceneGraph
{
    public LLMWorldBoundsType world_bounds;
    public List<LLMGameObject> game_objects;
}

[System.Serializable]
public class LLMGenerateRequestPayload
{
    public string prompt;
    public LLMCoordinate flock_position;
    public LLMSceneGraph scene_graph;
    public List<string> available_styles;
    public float temperature = 0.5f; 
    public int top_k = 10;          
    public float top_p = 0.6f;         
}

[System.Serializable]
public class LLMGenerationOutput
{
    public string style;
    public List<LLMVector> vectors;
} 