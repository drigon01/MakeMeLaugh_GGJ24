using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class DialogOptionsTable : ScriptableObject
{
    public List<string> Lines;
    
    public string GetRandomLine() => Lines[Random.Range(0, Lines.Count)];
}
