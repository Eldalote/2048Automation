using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    public NumBlocksManager NumBlocksManager;
    [SerializeField] private List<MoveDirection> _inputQueList = new List<MoveDirection>();
    [SerializeField] private bool _AutomatedMode = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) _inputQueList.Add(MoveDirection.Up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) _inputQueList.Add(MoveDirection.Down);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) _inputQueList.Add(MoveDirection.Left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) _inputQueList.Add(MoveDirection.Right);

        if (_inputQueList.Count > 0)
        {
            NumBlocksManager.ResultCode result = NumBlocksManager.GiveMoveInput(_inputQueList[0]);
            if (result == NumBlocksManager.ResultCode.GameOver) 
            {
                Debug.Log("Game over has been reported, action needed in Inputmanager.");
                return;
            }
            if (result == NumBlocksManager.ResultCode.Success)
            {
                _inputQueList.RemoveAt(0);
            }

        }
        
    }
}
