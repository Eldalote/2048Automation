using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private NumBlocksManager _numBlocksManager;
    private MoveDirection _commandToSend = new MoveDirection();
    [SerializeField] private bool _sendCommand = false;    
    [SerializeField] private bool _automatedMode = false;
    private UserInputActions _userInput;
    [SerializeField] private bool _managerReadyForInput = false;
    private AutomatedMoveGenerator _autoMoveGenerator;

    // Awake, find the NumBlockManager, get the player input component and subscribe to it's events.
    private void Awake()
    {
        _numBlocksManager = FindFirstObjectByType<NumBlocksManager>();
        _userInput = new UserInputActions();
        _userInput.GameInputs.Enable();
        _userInput.GameInputs.MoveUp.performed += MoveUp_performed;
        _userInput.GameInputs.MoveDown.performed += MoveDown_performed;
        _userInput.GameInputs.MoveLeft.performed += MoveLeft_performed;
        _userInput.GameInputs.MoveRight.performed += MoveRight_performed;

        // Subscribe to numbock manager events
        NumBlocksManager.readyForNewInput += ReadyForNewInput;
        NumBlocksManager.gameStarted += NewGameStarted;
        // Subscribe to automated system events
        AutomatedMoveGenerator.nextMove += ReceiveAutomatedInput; 
        // Create new automovegenerator.
        _autoMoveGenerator = new AutomatedMoveGenerator();
        
        
    }

    private void MoveRight_performed(InputAction.CallbackContext context)
    {
        // If the game is in manual mode (not automated) set the send command flags.
        if (!_automatedMode)
        {
            _commandToSend = MoveDirection.Right;
            _sendCommand = true;
        }
        
    }

    private void MoveLeft_performed(InputAction.CallbackContext context)
    {
        // If the game is in manual mode (not automated) set the send command flags.
        if (!_automatedMode)
        {
            _commandToSend = MoveDirection.Left;
            _sendCommand = true;
        }
    }

    private void MoveDown_performed(InputAction.CallbackContext context)
    {
        // If the game is in manual mode (not automated) set the send command flags.
        if (!_automatedMode)
        {
            _commandToSend = MoveDirection.Down;
            _sendCommand = true;
        }
    }

    private void MoveUp_performed(InputAction.CallbackContext context)
    {
        // If the game is in manual mode (not automated) set the send command flags.
        if (!_automatedMode)
        {
            _commandToSend = MoveDirection.Up;
            _sendCommand = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_managerReadyForInput && _sendCommand)
        {            
            SendCommand();

        }
        
    }
    
    // Public function called by "automated" toggle button on screen.
    public void ToggleAutomated(bool automated)
    {
        _automatedMode = automated;
        if (_automatedMode)
        {
            GetAutomatedInput();
        }
        
    }
    // Function that handles the request of automated input
    private void GetAutomatedInput()
    {
        ulong[] board = _numBlocksManager.GetCompactGamestate();
        ulong score = _numBlocksManager.GetScore();
        _autoMoveGenerator.GenerateNextMove(board, score);
    }
    // 
    private void ReadyForNewInput()
    {
        _managerReadyForInput = true;
        if(_automatedMode)
        {
            GetAutomatedInput();
        }
    }
    private void ReceiveAutomatedInput(MoveDirection direction)
    {
        if (_automatedMode)
        {            
            _commandToSend = direction;
            _sendCommand = true;            
        }
    }
    // 
    private void NewGameStarted()
    {
        _managerReadyForInput = true;
        if (_automatedMode)
        {
            GetAutomatedInput();
        }
    }

    private void SendCommand()
    {
        if (_numBlocksManager.GetGameOverStatus())
        {
            _sendCommand = false;
            _managerReadyForInput = false;
            return;
        }
        _managerReadyForInput = false;
        NumBlocksManager.ResultCode result = _numBlocksManager.GiveMoveInput(_commandToSend);
        if (result == NumBlocksManager.ResultCode.Success)
        {
            _sendCommand = false;          
            
        }
    }

}
