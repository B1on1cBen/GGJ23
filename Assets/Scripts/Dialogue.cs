using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "")]
public class Dialogue : ScriptableObject
{
    [TextArea] public string text;   
    public AudioClip voice;
}
