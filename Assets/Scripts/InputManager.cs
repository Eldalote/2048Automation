using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private NumBlocksManager _numBlocksManager;
    private MoveDirection _commandToSend = new MoveDirection();
    private bool _sendCommand = false;
    [SerializeField] private bool _AutomatedMode = false;
    private UserInputActions _userInput;

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
        
        
    }

    private void MoveRight_performed(InputAction.CallbackContext context)
    {
        _commandToSend = MoveDirection.Right;
        _sendCommand = true;
    }

    private void MoveLeft_performed(InputAction.CallbackContext context)
    {
        _commandToSend = MoveDirection.Left;
        _sendCommand = true;
    }

    private void MoveDown_performed(InputAction.CallbackContext context)
    {
        _commandToSend = MoveDirection.Down;
        _sendCommand = true;
    }

    private void MoveUp_performed(InputAction.CallbackContext context)
    {
        _commandToSend = MoveDirection.Up;
        _sendCommand = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        

        if (_sendCommand)
        {
            NumBlocksManager.ResultCode result = _numBlocksManager.GiveMoveInput(_commandToSend);
            if (result == NumBlocksManager.ResultCode.GameOver) 
            {
                Debug.Log("Game over has been reported, action needed in Inputmanager.");
                return;
            }
            if (result == NumBlocksManager.ResultCode.Success)
            {
                _sendCommand = false;
            }

        }
        
    }
}
