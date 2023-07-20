using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainGameManager : MonoBehaviour
{
    // Main game camera.
    [SerializeField] private Camera _gameCamera;
    // Sub units for the game.
    [SerializeField] private BackGroundGridBuilder _backGroundGridBuilder;
    [SerializeField] private NumBlocksManager _numBlocksManager;

    // General game variables.
    private int _gridSize = 4;
    private int _gridScale = 10;
    [SerializeField] private int _numBlockMoveSpeed = 25;
    private int _numBlockDisplayLayer = -1;


    void Start()
    {
        // Build backgroundgrid and center camera.
        _backGroundGridBuilder.CreateGrid(_gridSize, _gridScale);
        _gameCamera.transform.position = _backGroundGridBuilder.GetCenterOfGrid();
        // Initialize numBlockManager
        _numBlocksManager.Initialize(_gridSize, _gridScale, _numBlockMoveSpeed, _numBlockDisplayLayer);
    }
    
    
    

    public void RestartGame()
    {
        _numBlocksManager.StartNewGame();
    }
    
    

}
