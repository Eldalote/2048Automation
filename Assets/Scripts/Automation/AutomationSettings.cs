using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Automation/Settings")]
public class AutomationSettings : ScriptableObject
{
    public int SearchDepth;
    public int MaxSearchTimeMillis;
}
