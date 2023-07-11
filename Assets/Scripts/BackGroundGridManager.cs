using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundGridManager : MonoBehaviour
{
    [SerializeField] private int _gridWidth, _gridHeight;
    [SerializeField] private int _gridScale;

    [SerializeField] private GameObject _tilePrefab;
    [SerializeField] private Transform _cam;


    public void Start()
    {
        CreateGrid();
    }

    public void CreateGrid()
    {
        for (int x = 0; x < _gridWidth; x++)
        {
            for (int y = 0; y < _gridHeight; y++)
            {
                var spawnedTile = Instantiate(_tilePrefab, new Vector3(x * _gridScale, y * _gridScale), Quaternion.identity);
                spawnedTile.name = $"Tile {x} {y}";
            }
        }

        _cam.transform.position = new Vector3(_gridScale * ((float)_gridWidth / 2 - 0.5f), _gridScale * ((float)_gridHeight / 2 - 0.5f), -10);
    }
}
