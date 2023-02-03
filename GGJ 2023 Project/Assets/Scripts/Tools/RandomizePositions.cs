using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//--------------------------------------------------------------------------------------------
// This class will randomize the position of the supplied array of objects
// If no corresponding positions are set in the editor it will default to the existing 
// position of the game objects and shuffle them
//--------------------------------------------------------------------------------------------
public class RandomizePositions : MonoBehaviour
{
    [SerializeField] private Transform[] _objects;
    [SerializeField] private Vector3[] _positions;

    private List<Vector3> _shuffledPositions;

    void OnEnable ()
    {
        Initialize();
        Randomize();
    }

    public void Randomize ()
    {
        for (int i = 0; i < _objects.Length; i++)
        {
            _objects[i].position = _shuffledPositions[i];
        }
    }

    private void Initialize ()
    {
        _shuffledPositions = new List<Vector3>();

        // if we have no positions set in the inspector 
        if (_positions.Length <= 0)
        {
            GetPositionsForObjects();
        }
        // if there are more than 0 positions but also more objects then positions, default to object positions and log a warning
        else if (_positions.Length < _objects.Length)
        {
            Debug.LogWarning("WARNING: an array of positions was set in the inspector but there are more objects than positions. Defaulting to object positions for randomization. ", gameObject);
            GetPositionsForObjects();
        }
        else 
        {
            for (int i = 0; i < _positions.Length; i++)
            {
                _shuffledPositions.Add(_positions[i]);
            }
        }

        ExtensionMethods.Shuffle(_shuffledPositions);
    }

    // Fills the _shuffledPositions List with the positions of the objects in the _objects array
    private void GetPositionsForObjects ()
    {
        for (int i = 0; i < _objects.Length; i++)
        {
            _shuffledPositions.Add(_objects[i].position);
        }
    }
}