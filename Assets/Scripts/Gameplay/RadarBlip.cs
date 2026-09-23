using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class RadarBlip : MonoBehaviour
{
    [Header("Visuals")]
    public TextMeshPro wordText; 
    
    [Header("Radar Sweep Fading")]
    public float textFadeSpeed = 0.8f; 
    public bool isTargeted = false; 

    [Header("Blip Data")]
    public string targetWord;
    public string fullMorseSequence;
    public int currentSequenceIndex = 0; 
    public int pointValue = 10; 

    private Transform centerTarget;
    private float moveSpeed;
    private RadarManager manager;
    private Camera mainCamera;
    private float textHeightOffset;

    private static readonly Dictionary<char, string> morseDictionary = new Dictionary<char, string>()
    {
        {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."},
        {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"},
        {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"},
        {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."}, {'S', "..."}, {'T', "-"},
        {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"}, {'Z', "--.."}
    };

    public void Setup(string word, Transform target, float speed, RadarManager myManager)
    {
        targetWord = word.ToUpper();
        centerTarget = target;
        moveSpeed = speed;
        manager = myManager; 
        wordText.text = targetWord;

        mainCamera = Camera.main;
        if (wordText != null)
        {
            textHeightOffset = wordText.transform.localPosition.y; 
        }

        wordText.alpha = 0f; 

        fullMorseSequence = "";
        foreach (char c in targetWord)
        {
            if (morseDictionary.ContainsKey(c)) fullMorseSequence += morseDictionary[c];
        }
    }

    void Update()
    {
        if (centerTarget != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, centerTarget.position, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, centerTarget.position) < 0.2f)
            {
                manager.ReportDamage(this); 
            }
        }

        if (!isTargeted && wordText.alpha > 0)
        {
            wordText.alpha -= Time.deltaTime * textFadeSpeed;
        }
        else if (isTargeted)
        {
            wordText.alpha = 1f; 
        }
    }

    void LateUpdate()
    {
        if (mainCamera != null && wordText != null)
        {
            wordText.transform.rotation = mainCamera.transform.rotation;
            wordText.transform.position = transform.position 
                                          + (mainCamera.transform.up * textHeightOffset) 
                                          - (mainCamera.transform.forward * 0.5f); 
        }
    }

    public void PingRadar()
    {
        wordText.alpha = 1f; 
    }

    public bool ReceiveInput(string inputSymbol)
    {
        string expectedSymbol = fullMorseSequence[currentSequenceIndex].ToString();

        if (inputSymbol == expectedSymbol)
        {
            currentSequenceIndex++; 
            UpdateTextVisuals(); 
            
            if (currentSequenceIndex >= fullMorseSequence.Length)
            {
                manager.ReportDestroyed(this, pointValue); 
            }
            return true; 
        }
        else
        {
            currentSequenceIndex = 0;
            UpdateTextVisuals(); 
            return false;
        }
    }

    private void UpdateTextVisuals()
    {
        int completedLetters = 0;
        int morseCount = 0;

        for (int i = 0; i < targetWord.Length; i++)
        {
            char c = targetWord[i];
            if (morseDictionary.ContainsKey(c))
            {
                morseCount += morseDictionary[c].Length;
            }

            if (currentSequenceIndex >= morseCount)
            {
                completedLetters = i + 1;
            }
            else
            {
                break;
            }
        }

        string greenPart = targetWord.Substring(0, completedLetters);
        string whitePart = targetWord.Substring(completedLetters);

        if (completedLetters > 0)
        {
            wordText.text = $"<color=green>{greenPart}</color>{whitePart}";
        }
        else
        {
            wordText.text = targetWord;
        }
    }
    public string GetAcceptedMorse()
    {
        if (string.IsNullOrEmpty(fullMorseSequence) || currentSequenceIndex == 0)
        {
            return "";
        }

        string formattedInput = "";
        int trackedIndex = 0;

        for (int i = 0; i < targetWord.Length; i++)
        {
            char c = targetWord[i];
            if (morseDictionary.ContainsKey(c))
            {
                string letterMorse = morseDictionary[c];
                
                if (currentSequenceIndex >= trackedIndex + letterMorse.Length)
                {
                    formattedInput += letterMorse + " ";
                    trackedIndex += letterMorse.Length;
                }
                else if (currentSequenceIndex > trackedIndex)
                {
                    int partialLength = currentSequenceIndex - trackedIndex;
                    formattedInput += letterMorse.Substring(0, partialLength);
                    break; 
                }
                else
                {
                    break; 
                }
            }
        }

        return formattedInput; 
    }
}