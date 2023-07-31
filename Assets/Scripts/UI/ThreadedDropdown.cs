using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ThreadedDropdown : MonoBehaviour
{    
    [SerializeField] private InputManager _inputManager;
    
    // Start is called before the first frame update
    void Start()
    {
        var dropDown = transform.GetComponent<TMP_Dropdown>();
        dropDown.options.Clear();
        List<string> dropDownOptions = new List<string>();
        dropDownOptions.Add("Not-Threaded");
        dropDownOptions.Add("Threaded top level");
        dropDownOptions.Add("Threaded two levels");

        foreach (string option in dropDownOptions)
        {
            dropDown.options.Add(new TMP_Dropdown.OptionData() { text = option});
        }
        dropDown.onValueChanged.AddListener(delegate { DropDownOptionSelected(dropDown); });
    }

    void DropDownOptionSelected(TMP_Dropdown dropdown)
    {
        _inputManager.ChangeThreadedOptions(dropdown.value);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
