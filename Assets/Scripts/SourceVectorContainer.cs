using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SourceVectorContainer : MonoBehaviour
{
    [SerializeField] private GameObject sourcePointAndVectorPrefab;  // Reference to the prefab
    private GridRenderer gridRenderer;  // We'll find this at runtime
    private SourcePointAndVector[] _sourcePointsAndVectors;
    private Vector3[] _sourcePositions;
    public Vector3[] SourcePositions => _sourcePositions != null ? _sourcePositions : null;
    private Vector3[] _sourceVectors;
    public Vector3[] SourceVectors => _sourceVectors != null ? _sourceVectors : null;

    void Awake()
    {
        if (sourcePointAndVectorPrefab == null)
        {
            Debug.LogError("SourcePointAndVector prefab is not assigned in the inspector!");
        }

        // Find the GridRenderer in the scene
        gridRenderer = FindObjectOfType<GridRenderer>();
        if (gridRenderer == null)
        {
            Debug.LogError("Could not find GridRenderer in scene!");
        }

        _sourcePointsAndVectors = GetComponentsInChildren<SourcePointAndVector>();
        _sourcePositions = new Vector3[_sourcePointsAndVectors.Length];
        _sourceVectors = new Vector3[_sourcePointsAndVectors.Length];

        for (int i = 0; i < _sourcePointsAndVectors.Length; i++)
        {
            //Debug.Log("_sourcePointsAndVectors[" + i + "]");
            //Debug.Log(_sourcePointsAndVectors[i].SourcePoint);
            //Debug.Log(_sourcePointsAndVectors[i].SourceVector);
            _sourcePositions[i] = _sourcePointsAndVectors[i].SourcePoint;
            _sourceVectors[i] = _sourcePointsAndVectors[i].SourceVector;
        }
    }

    public void ReplaceSourceVectors(List<LLMVector> newVectors)
    {
        if (sourcePointAndVectorPrefab == null)
        {
            Debug.LogError("Cannot replace vectors: SourcePointAndVector prefab is not assigned!");
            return;
        }

        StartCoroutine(ReplaceVectorsCoroutine(newVectors));
    }

    private IEnumerator ReplaceVectorsCoroutine(List<LLMVector> newVectors)
    {
        // First, destroy all existing source vectors
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Wait a frame for the destroy to complete
        yield return null;

        // Create new source vectors for each vector in the response
        for (int i = 0; i < newVectors.Count; i++)
        {
            var vector = newVectors[i];
            
            // Instantiate the prefab
            GameObject sourceVectorObj = Instantiate(sourcePointAndVectorPrefab);
            sourceVectorObj.name = $"SourceVector_{i}";  // Give it a meaningful name
            sourceVectorObj.transform.SetParent(transform);
            
            // Get the Point and VectorHead children
            Transform point = sourceVectorObj.transform.Find("Point");
            Transform vectorHead = sourceVectorObj.transform.Find("VectorHead");
            
            if (point != null && vectorHead != null)
            {
                // Set positions
                point.position = new Vector3(vector.s.x, 0, vector.s.z); // y=0 for points
                vectorHead.position = new Vector3(vector.e.x, vector.e.y, vector.e.z);
                
                // Get and set up the source vector component
                SourcePointAndVector sourceVector = sourceVectorObj.GetComponent<SourcePointAndVector>();
                if (sourceVector != null)
                {
                    sourceVector.arrowGumble = sourceVectorObj;
                    sourceVector.sourcePointController = point.gameObject;
                    sourceVector.sourceVectorController = vectorHead.gameObject;
                }
            }
            else
            {
                Debug.LogError("SourcePointAndVector prefab is missing Point or VectorHead children");
                Destroy(sourceVectorObj);
            }
        }

        // Wait a frame for all components to initialize
        yield return null;

        // Refresh the arrays
        _sourcePointsAndVectors = GetComponentsInChildren<SourcePointAndVector>();
        _sourcePositions = new Vector3[_sourcePointsAndVectors.Length];
        _sourceVectors = new Vector3[_sourcePointsAndVectors.Length];
        
        for (int i = 0; i < _sourcePointsAndVectors.Length; i++)
        {
            _sourcePositions[i] = _sourcePointsAndVectors[i].SourcePoint;
            _sourceVectors[i] = _sourcePointsAndVectors[i].SourceVector;
        }

        // Update the vector field
        if (gridRenderer != null)
        {
            gridRenderer.UpdateVectorField();
        }
        else
        {
            Debug.LogError("GridRenderer reference is missing!");
        }
    }

    // Update is called once per frame
    void Update()
    {


    }
}
