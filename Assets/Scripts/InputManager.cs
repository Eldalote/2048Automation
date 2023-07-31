using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

public class InputManager : MonoBehaviour
{
    private NumBlocksManager _numBlocksManager;
    private MoveDirection _commandToSend = new MoveDirection();
    [SerializeField] private bool _sendCommand = false;    
    [SerializeField] private bool _automatedMode = false;
    private UserInputActions _userInput;
    [SerializeField] private bool _managerReadyForInput = false; 
    [SerializeField] private AutomationSettings _automationSettings;
    [SerializeField] private bool threaded = false;
    [SerializeField] private SearcherConsole _searcherConsole;
    private Process _searcherProcess;
    private Process _builderProcess;
    private StreamWriter _searcherStreamWriter;
    private string _nextSearcherCommand;
    private int _searcherThreadedOption = 0;
    private bool _searcherWaitingForCommand = false;

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
    public void ToggleAutomated()
    {
        _automatedMode = !_automatedMode;
        if (_automatedMode)
        {
            GetAutomatedInput();
        }
        
    }
    // Function that handles the request of automated input
    private void GetAutomatedInput()
    {
        ulong[] board = _numBlocksManager.GetCompactGamestate();
        
        string command = "Search LSB:" + board[0].ToString() + " MSB:" + board[1].ToString();
        SendSearcherCommand(command);
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
                  
            _commandToSend = direction;
            _sendCommand = true;            
        
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

    public void OneAutoMove()
    {
        GetAutomatedInput() ;
        
        
    }

    public void StartSearcher()
    {
        // If a search process is already running, kill it, then start a new one.
        if (_searcherProcess != null)
        {
            _searcherProcess.Kill();
        }

        // Get path to folder, this is the /Assets folder, so the last part needs to be removed.
        String searcherPath = Application.dataPath;            
        searcherPath = searcherPath.Substring(0, searcherPath.LastIndexOf('/'));
        // Point the path to the searcher exe file.
        searcherPath += "/SearchEngine/bin/Release/net7.0/SearchEngine.exe";
        // Set process start options
        ProcessStartInfo searcherStartInfo = new ProcessStartInfo(searcherPath);
        searcherStartInfo.CreateNoWindow = false;
        searcherStartInfo.UseShellExecute = false;
        searcherStartInfo.RedirectStandardOutput = true;
        searcherStartInfo.RedirectStandardInput = true;

        // Start the process.
        try
        {
            _searcherProcess = Process.Start(searcherStartInfo);
        }
        catch (Exception e)
        {
            Debug.Log($"Error starting Searcher: {e}");
        }
        // Set a stream writer to the standard input stream, so this can talk to the searcher.
        _searcherStreamWriter = _searcherProcess.StandardInput;
        // Set a function to intercept received data from the standard output from the searcher, so it can talk to this.
        _searcherProcess.OutputDataReceived += ReceiveSearcherMessage;
        _searcherProcess.BeginOutputReadLine();
                
    }

    public void BuildSearcher()
    {
        // Get path to folder, this is the /Assets folder, so the last part needs to be removed.
        String builderPath = Application.dataPath;        
        builderPath = builderPath.Substring(0, builderPath.LastIndexOf('/'));
        // Point the path to the auto friendly build batfile.
        builderPath += "/SearchEngine/BuildAuto.bat";
        // Set process options
        ProcessStartInfo builderProcessInfo = new ProcessStartInfo(builderPath);
        builderProcessInfo.CreateNoWindow = false;
        builderProcessInfo.UseShellExecute = true;
        builderProcessInfo.RedirectStandardOutput = false;
        builderProcessInfo.RedirectStandardInput = false;
        // Start the process (bat file)
        try
        {
            _builderProcess = Process.Start(builderProcessInfo);
        }
        catch (Exception e)
        {
            Debug.Log($"Error starting Searcher build: {e}");
        }
    }

    private void ReceiveSearcherMessage(object sendingProcess, DataReceivedEventArgs outLine)
    {
        if (!string.IsNullOrEmpty(outLine.Data))
        {            
            // If the input is not nothing, print it to the "Onscreen console".
            _searcherConsole.PrintToOnScreenConsole(outLine.Data);
            // Split the input into substrings by space
            string[] subStrings = outLine.Data.Split(" ");
            // Switch statement for the first substring
            switch (subStrings[0])
            {
                case "BestMove":
                    {
                        MoveDirection direction = MoveDirection.None;
                        // switch statement for the directions.
                        switch (subStrings[1])
                        {
                            case "Up":
                                {
                                    direction = MoveDirection.Up;
                                    break;
                                }
                            case "Down":
                                {
                                    direction= MoveDirection.Down;
                                    break;
                                }
                            case "Left":
                                {
                                    direction = MoveDirection.Left;
                                    break;
                                }
                            case "Right":
                                {
                                    direction= MoveDirection.Right;
                                    break;
                                }
                            
                        }
                        // Send the input to the blockmanager.
                        ReceiveAutomatedInput(direction);
                        break;
                    }
                case "Ready":
                    {
                        // Try sending a waiting command, if none was send, set the flag that the seacher is waiting.
                        _searcherWaitingForCommand = !SendWaitingSearcherCommand();
                        break;
                    }
                
            }
        }
    }

    public void ChangeThreadedOptions(int options)
    {
        _searcherThreadedOption = options;
    }

    public void DoBenchMark()
    {
        SendSearcherCommand("Benchmark");
    }

    private void SendSearcherCommand(string command)
    {
        if (_searcherWaitingForCommand)
        {
            _searcherStreamWriter.WriteLine(_nextSearcherCommand);
        }
        else
        {
            _nextSearcherCommand = command;
        }
    }

    private bool SendWaitingSearcherCommand()
    {
        if (_nextSearcherCommand.Length > 1)
        {
            _searcherStreamWriter.WriteLine(_nextSearcherCommand);
            _nextSearcherCommand = "";
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool GetAutomatedStatus()
    { return _automatedMode; }

}
