using System.Collections.Generic;
using UnityEngine;

public class WordBank : MonoBehaviour
{
    [Header("Data")]
    public TextAsset wordFile;

    private List<string> shortWords = new List<string>();
    private List<string> mediumWords = new List<string>();
    private List<string> longWords = new List<string>();

    void Awake()
    {
        LoadWords();
    }

    void LoadWords()
    {
        if (wordFile == null) return;

        string[] allWords = wordFile.text.Split('\n');

        foreach (string word in allWords)
        {
            string cleanWord = word.Trim().ToUpper(); 
            if (string.IsNullOrEmpty(cleanWord)) continue;

            if (cleanWord.Length <= 3)
            {
                shortWords.Add(cleanWord); 
            }
            else if (cleanWord.Length <= 6)
            {
                mediumWords.Add(cleanWord);
            }
            else
            {
                longWords.Add(cleanWord);
            }
        }
    }

    public string GetRandomWord(string enemyType)
    {
        if (enemyType == "Torpedo" && shortWords.Count > 0)
            return shortWords[Random.Range(0, shortWords.Count)];
            
        if (enemyType == "Leviathan" && longWords.Count > 0)
            return longWords[Random.Range(0, longWords.Count)];
            
        return mediumWords[Random.Range(0, mediumWords.Count)];
    }
}