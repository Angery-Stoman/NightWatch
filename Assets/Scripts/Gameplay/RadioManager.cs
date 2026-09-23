using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class RadioManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI clipboardText;
    public TextMeshProUGUI frequencyDisplay;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverScoreText; 

    [Header("Life UI (3D LEDs)")]
    public GameObject[] lifeLEDs;
    public Material ledOnMaterial;
    public Material ledOffMaterial;

    [Header("Audio")]
    public AudioSource staticAudio;
    public AudioSource telegraphAudio;
    public AudioClip dotSound;
    public AudioClip dashSound;
    public AudioClip tuningStaticBurst;
    public AudioClip correctSound;
    public AudioClip errorSound;

    [Header("Radio Settings")]
    public TextAsset wordBankFile; 
    public float tuningDelay = 0.6f;
    public float morseUnitTime = 0.12f; 
    
    [Header("Tuning Indicator (The Red Slider)")]
    public Transform tuningIndicator; 
    public Transform dialLeftBound;   
    public Transform dialRightBound;  
    
    private int[] militaryFrequencies = { 
        3105, 3885, 4220, 5130, 6210, 
        7040, 7280, 8364, 8440, 9050 
    };

    private List<string> allAvailableWords = new List<string>();
    private string[] channelWords = new string[10];
    private List<string> targetSequence = new List<string>();
    
    private int currentChannel = 0;
    private int currentTargetIndex = 0;
    private int currentLives = 3;
    private int sequencesDecoded = 0; 
    private bool isTuning = false;
    private bool isGameOver = false;

    private Coroutine activeTransmission;

    private static readonly Dictionary<char, string> morseDictionary = new Dictionary<char, string>()
    {
        {'A', ".-"}, {'B', "-..."}, {'C', "-.-."}, {'D', "-.."}, {'E', "."},
        {'F', "..-."}, {'G', "--."}, {'H', "...."}, {'I', ".."}, {'J', ".---"},
        {'K', "-.-"}, {'L', ".-.."}, {'M', "--"}, {'N', "-."}, {'O', "---"},
        {'P', ".--."}, {'Q', "--.-"}, {'R', ".-."}, {'S', "..."}, {'T', "-"},
        {'U', "..-"}, {'V', "...-"}, {'W', ".--"}, {'X', "-..-"}, {'Y', "-.--"}, {'Z', "--.."}
    };

    void Start()
    {
        currentLives = lifeLEDs.Length;
        sequencesDecoded = 0;
        
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameOverScoreText != null) gameOverScoreText.text = ""; 

        foreach (GameObject led in lifeLEDs) SetLEDMaterial(led, ledOnMaterial);

        ParseWordBank();
        GenerateRadioPuzzle();
        UpdateClipboardUI();
        
        UpdateIndicatorPosition(militaryFrequencies[0]);
        TuneToChannel(0, true);
    }

    void Update()
    {
        if (isGameOver || isTuning) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            int nextChannel = (currentChannel + 1) % 10;
            TuneToChannel(nextChannel, false);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            int prevChannel = currentChannel - 1;
            if (prevChannel < 0) prevChannel = 9;
            TuneToChannel(prevChannel, false);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            CheckGuess();
        }
    }

    void ParseWordBank()
    {
        if (wordBankFile != null)
        {
            string[] words = wordBankFile.text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string w in words)
            {
                allAvailableWords.Add(w.Trim().ToUpper());
            }
        }
        else
        {
            allAvailableWords.Add("ERROR"); 
        }
    }

    void GenerateRadioPuzzle()
    {
        int sequenceLength = Random.Range(2, 6);
        targetSequence.Clear();

        for (int i = 0; i < sequenceLength; i++)
        {
            int r = Random.Range(0, allAvailableWords.Count);
            targetSequence.Add(allAvailableWords[r]);
        }

        List<string> availableWords = new List<string>(targetSequence);
        
        while (availableWords.Count < 10)
        {
            int r = Random.Range(0, allAvailableWords.Count);
            string randomWord = allAvailableWords[r];
            if (!availableWords.Contains(randomWord)) availableWords.Add(randomWord);
        }

        for (int i = 0; i < availableWords.Count; i++)
        {
            string temp = availableWords[i];
            int randomIndex = Random.Range(i, availableWords.Count);
            availableWords[i] = availableWords[randomIndex];
            availableWords[randomIndex] = temp;
        }

        for (int i = 0; i < 10; i++)
        {
            channelWords[i] = availableWords[i];
        }
    }

    void TuneToChannel(int nextChannelIndex, bool instant)
    {
        int previousChannel = currentChannel;
        currentChannel = nextChannelIndex;

        if (activeTransmission != null) StopCoroutine(activeTransmission);
        telegraphAudio.Stop();

        if (instant)
        {
            frequencyDisplay.text = militaryFrequencies[currentChannel] + " kHz";
            UpdateIndicatorPosition(militaryFrequencies[currentChannel]);
            activeTransmission = StartCoroutine(TransmitMorseLoop(channelWords[currentChannel]));
        }
        else
        {
            StartCoroutine(TuningRoutine(previousChannel, currentChannel));
        }
    }

    private IEnumerator TuningRoutine(int startChannel, int targetChannel)
    {
        isTuning = true;
        
        if (staticAudio != null && tuningStaticBurst != null)
        {
            staticAudio.clip = tuningStaticBurst;
            float maxStartTime = Mathf.Max(0f, tuningStaticBurst.length - tuningDelay);
            staticAudio.time = Random.Range(0f, maxStartTime);
            staticAudio.Play();
        }

        float elapsed = 0f;
        int startFreq = militaryFrequencies[startChannel];
        int targetFreq = militaryFrequencies[targetChannel];

        while (elapsed < tuningDelay)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tuningDelay;
            float easeT = Mathf.SmoothStep(0f, 1f, t);
            
            int currentFreq = Mathf.RoundToInt(Mathf.Lerp(startFreq, targetFreq, easeT));
            frequencyDisplay.text = currentFreq + " kHz";
            
            UpdateIndicatorPosition(currentFreq);
            
            yield return null; 
        }

        frequencyDisplay.text = targetFreq + " kHz";
        UpdateIndicatorPosition(targetFreq);
        
        if (staticAudio != null) staticAudio.Stop(); 

        activeTransmission = StartCoroutine(TransmitMorseLoop(channelWords[targetChannel]));
        isTuning = false;
    }

    void UpdateIndicatorPosition(int currentFrequency)
    {
        if (tuningIndicator == null || dialLeftBound == null || dialRightBound == null) return;

        float minFreq = militaryFrequencies[0]; 
        float maxFreq = militaryFrequencies[militaryFrequencies.Length - 1]; 

        float percentage = Mathf.InverseLerp(minFreq, maxFreq, currentFrequency);
        tuningIndicator.position = Vector3.Lerp(dialLeftBound.position, dialRightBound.position, percentage);
    }

    private IEnumerator TransmitMorseLoop(string word)
    {
        Debug.Log($"<color=yellow>[CHEAT]</color> Channel Word: <b>{word}</b> | Looking for: <b>{targetSequence[currentTargetIndex]}</b>");
        
        string sequence = "";
        foreach (char c in word)
        {
            if (morseDictionary.ContainsKey(c)) sequence += morseDictionary[c] + " ";
        }

        while (true)
        {
            foreach (char symbol in sequence)
            {
                if (symbol == '.')
                {
                    telegraphAudio.PlayOneShot(dotSound);
                    yield return new WaitForSeconds(morseUnitTime);
                }
                else if (symbol == '-')
                {
                    telegraphAudio.PlayOneShot(dashSound);
                    yield return new WaitForSeconds(morseUnitTime * 3f);
                }
                else if (symbol == ' ')
                {
                    yield return new WaitForSeconds(morseUnitTime * 3f);
                    continue; 
                }

                yield return new WaitForSeconds(morseUnitTime);
            }

            yield return new WaitForSeconds(morseUnitTime * 7f);
        }
    }

    void CheckGuess()
    {
        string selectedWord = channelWords[currentChannel];
        string expectedWord = targetSequence[currentTargetIndex];

        if (selectedWord == expectedWord)
        {
            if (telegraphAudio != null && correctSound != null) telegraphAudio.PlayOneShot(correctSound);
            
            currentTargetIndex++;
            UpdateClipboardUI();

            if (currentTargetIndex >= targetSequence.Count)
            {
                StartCoroutine(HandleSequenceComplete());
            }
        }
        else
        {
            if (telegraphAudio != null && errorSound != null) telegraphAudio.PlayOneShot(errorSound);
            
            currentLives--;

            if (currentLives >= 0 && currentLives < lifeLEDs.Length)
            {
                StartCoroutine(FlickerAndDie(lifeLEDs[currentLives]));
            }

            if (currentLives <= 0)
            {
                TriggerGameOver();
            }
        }
    }

    private IEnumerator HandleSequenceComplete()
    {
        sequencesDecoded++;

        yield return new WaitForSeconds(1.5f);

        currentTargetIndex = 0;
        GenerateRadioPuzzle();
        UpdateClipboardUI();

        TuneToChannel(currentChannel, true); 
    }

    void UpdateClipboardUI()
    {
        string displayText = "";

        for (int i = 0; i < targetSequence.Count; i++)
        {
            if (i < currentTargetIndex)
            {
                displayText += $"<color=green>{targetSequence[i]}</color>\n";
            }
            else if (i == currentTargetIndex)
            {
                displayText += $"> {targetSequence[i]} <\n";
            }
            else
            {
                displayText += $"{targetSequence[i]}\n";
            }
        }

        clipboardText.text = displayText;
    }

    private IEnumerator FlickerAndDie(GameObject led)
    {
        if (led == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            SetLEDMaterial(led, ledOffMaterial);
            yield return new WaitForSeconds(0.05f);
            SetLEDMaterial(led, ledOnMaterial);
            yield return new WaitForSeconds(0.05f);
        }
        SetLEDMaterial(led, ledOffMaterial);
    }

    private void SetLEDMaterial(GameObject ledParent, Material mat)
    {
        MeshRenderer[] renderers = ledParent.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer r in renderers)
        {
            r.material = mat;
        }
    }

    void TriggerGameOver()
    {
        isGameOver = true;
        if (activeTransmission != null) StopCoroutine(activeTransmission);
        
        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Messages Intercepted: " + sequencesDecoded;
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenuScene"); 
    }
}