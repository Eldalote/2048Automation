using System.Collections.Generic;
using UnityEngine;

public class BackGroundGridBuilder : MonoBehaviour
{
    // Unity game object refences.
    [SerializeField] private GameObject _backGroundTile;
    private List<GameObject> _backGroundTileList = new List<GameObject>();
    private Vector3 _centerOfGrid = Vector3.zero;


    public void Start()
    {
       
    }
    // Build the grid out of background tiles.
    public void CreateGrid(int gridSize, int gridScale)
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {                
                GameObject spawnedTile = Instantiate(_backGroundTile, gameObject.transform);
                spawnedTile.transform.position = new Vector3(x * gridScale, y * gridScale);
                spawnedTile.name = $"Background tile {x} {y}";
                _backGroundTileList.Add(spawnedTile);
            }
        }
        _backGroundTile.gameObject.SetActive(false);
        _centerOfGrid = new Vector3(gridScale * ((float)gridSize / 2 - 0.5f), gridScale * ((float)gridSize / 2 - 0.5f), -10);
    }
    // Get the center of the grid, (position to center the camera on).
    public Vector3 GetCenterOfGrid() { return _centerOfGrid; }
}
