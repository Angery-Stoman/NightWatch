using UnityEngine;

[CreateAssetMenu(fileName = "NewMorseLetter", menuName = "MorseCode/Letter")]
public class MorseLetter : ScriptableObject
{
    public string letter;          
    public string sequence;        
    public string mnemonic;        
    public AudioClip easyAudio;
    public AudioClip mediumAudio;
    public AudioClip hardAudio; 
}