using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MorseGameManager : MonoBehaviour
{
    [Header("UI References (Clipboard)")]
    public TextMeshProUGUI letterText;
    public TextMeshProUGUI mnemonicText;
    public TextMeshProUGUI sequenceText;
    public TextMeshProUGUI inputText;

    [Header("UI References (Screen)")]
    public GameObject victoryPanel;
    public GameObject gameOverPanel; 

    [Header("Tutorial Overlay")]
    public GameObject tutorialOverlayPanel;
    private bool isTutorialActive = false;

    [Header("Challenge Mode Rules")]
    public int maxLives = 3;
    private int currentLives;
    public float timePerLetter = 10f;
    private float timeRemaining;
    private bool isPlaying = false; 

    [Header("Challenge Mode 3D Objects")]
    public TextMeshProUGUI clockText; 
    public MeshRenderer[] lifeBulbs;  
    public Material bulbOnMaterial;   
    public Material bulbOffMaterial;  
    
    [Header("3D Environment")]
    public Light lampLight;
    public MeshRenderer lampBulbRenderer;
    public Material lampBulbOnMaterial;
    public Material lampBulbOffMaterial;
    public int bulbMaterialIndex = 6; 
    private bool isLampBulbOn = false; 
    public float dotFlashIntensity = 1f;
    public float dashFlashIntensity = 5f;
    public float dotFadeSpeed = 10f;
    public float dashFadeSpeed = 5f;
    private float currentFadeSpeed = 7f;
    
    private float morseUnitTime = 0.1f; 
    
    private Coroutine manualFlashCoroutine;
    private bool isTransitioning = false;
    public AudioSource telegraphAudio;
    
    [Header("Input Audio")]
    public AudioSource inputAudio;
    public AudioClip dotSound;
    public AudioClip dashSound;

    [Header("Error Audio")]
    public AudioSource errorAudio;
    public AudioClip errorSound;

    [Header("Data")]
    public List<MorseLetter> alphabet;
    private int currentLetterIndex = 0;
    private string currentInput = "";
    private List<int> availableChallengeIndices = new List<int>();

    void Start()
    {
        SetupDifficulty();

        if (MenuManager.selectedMode == MenuManager.GameMode.Learning)
        {
            SetupLearningMode();
        }
        else if (MenuManager.selectedMode == MenuManager.GameMode.VisualChallenge)
        {
            SetupChallengeMode(); 
        }
        else if (MenuManager.selectedMode == MenuManager.GameMode.AudioChallenge)
        {
            SetupChallengeMode(); 
        }
    }

    void SetupDifficulty()
    {
        if (MenuManager.selectedDifficulty == MenuManager.Difficulty.Easy)
        {
            morseUnitTime = 1.2f / 6f; 
        }
        else if (MenuManager.selectedDifficulty == MenuManager.Difficulty.Medium)
        {
            morseUnitTime = 1.2f / 9f; 
        }
        else 
        {
            morseUnitTime = 1.2f / 12f; 
        }
    }

    void SetupLearningMode()
    {
        currentLetterIndex = 0;
        isPlaying = true;
        
        if (MenuManager.selectedDifficulty == MenuManager.Difficulty.Easy)
        {
            isTutorialActive = true;
            if (tutorialOverlayPanel != null) tutorialOverlayPanel.SetActive(true);
        }
        else
        {
            if (tutorialOverlayPanel != null) tutorialOverlayPanel.SetActive(false);
            UpdateClipboardUI();
        }
    }
    
    void SetupChallengeMode()
    {
        if (tutorialOverlayPanel != null) tutorialOverlayPanel.SetActive(false);
        
        currentLives = maxLives;
        timeRemaining = timePerLetter;
        isPlaying = true;
        
        UpdateLivesUI(); 

        availableChallengeIndices.Clear();
        for (int i = 0; i < alphabet.Count; i++)
        {
            availableChallengeIndices.Add(i);
        }

        int randomIndex = Random.Range(0, availableChallengeIndices.Count);
        currentLetterIndex = availableChallengeIndices[randomIndex];
        
        availableChallengeIndices.RemoveAt(randomIndex);
        
        UpdateClipboardUI();
    }

    void Update()
    {
        lampLight.intensity = Mathf.Lerp(lampLight.intensity, 0f, Time.deltaTime * currentFadeSpeed);

        if (lampBulbRenderer != null)
        {
            if (lampLight.intensity > 0.1f && !isLampBulbOn)
            {
                Material[] mats = lampBulbRenderer.materials;
                mats[bulbMaterialIndex] = lampBulbOnMaterial;
                lampBulbRenderer.materials = mats;      
                isLampBulbOn = true;
            }
            else if (lampLight.intensity <= 0.1f && isLampBulbOn)
            {
                Material[] mats = lampBulbRenderer.materials;
                mats[bulbMaterialIndex] = lampBulbOffMaterial;
                lampBulbRenderer.materials = mats;
                
                isLampBulbOn = false;
            }
        }

        if (MenuManager.selectedMode == MenuManager.GameMode.VisualChallenge || 
            MenuManager.selectedMode == MenuManager.GameMode.AudioChallenge)
        {
            if (isPlaying && !isTransitioning && !isTutorialActive)
            {
                timeRemaining -= Time.deltaTime;
                if (clockText != null) clockText.text = timeRemaining.ToString("F1");

                if (timeRemaining <= 0)
                {
                    GameOver();
                }
            }
        }

        if (MenuManager.selectedMode == MenuManager.GameMode.AudioChallenge)
        {
            HandleTranslationInput();
        }
        else
        {
            HandleMorseInput();
        }
    }

    void HandleMorseInput()
    {
        if (isTransitioning || !isPlaying || isTutorialActive) return;

        if (Input.GetKeyDown(KeyCode.Period))
        {
            AddInput(".");
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            AddInput("-");
        }
    }

    void HandleTranslationInput()
    {
        if (isTransitioning || !isPlaying || isTutorialActive) return;

        foreach (char c in Input.inputString)
        {
            if (char.IsLetter(c))
            {
                CheckTranslation(c.ToString().ToUpper());
            }
        }
    }

    void CheckTranslation(string typedLetter)
    {
        string targetLetter = alphabet[currentLetterIndex].letter;
        
        inputText.text = typedLetter; 

        if (typedLetter == targetLetter)
        {
            Debug.Log("Correct Translation!");
            StartCoroutine(TransitionToNextLetter());
        }
        else
        {
            Debug.Log("Mistake! Wrong letter.");
            
            currentLives--;

            if (currentLives >= 0 && currentLives < lifeBulbs.Length)
            {
                StartCoroutine(FlickerAndDie(lifeBulbs[currentLives]));
            }

            timeRemaining = timePerLetter;
            PlayErrorSound();

            if (currentLives <= 0)
            {
                GameOver();
            }
        }
    }

    void AddInput(string symbol)
    {
        currentInput += symbol;
        inputText.text = currentInput;

        if (manualFlashCoroutine != null)
        {
            StopCoroutine(manualFlashCoroutine);
        }

        manualFlashCoroutine = StartCoroutine(ManualFlash(symbol));

        if (inputAudio != null)
        {
            inputAudio.Stop(); 

            if (symbol == "." && dotSound != null)
            {
                inputAudio.clip = dotSound;
                inputAudio.Play();
            }
            else if (symbol == "-" && dashSound != null)
            {
                inputAudio.clip = dashSound;
                inputAudio.Play();
            }
        }

        CheckSequence();
    }

    void CheckSequence()
    {
        string targetSequence = alphabet[currentLetterIndex].sequence;

        if (currentInput == targetSequence)
        {
            Debug.Log("Correct!");
            StartCoroutine(TransitionToNextLetter());
        }
        else if (!targetSequence.StartsWith(currentInput))
        {
            Debug.Log("Mistake! Resetting input.");
            currentInput = "";
            inputText.text = currentInput;
            PlayErrorSound();
            
            if (MenuManager.selectedMode == MenuManager.GameMode.VisualChallenge)
            {
                currentLives--;

                if (currentLives >= 0 && currentLives < lifeBulbs.Length)
                {
                    StartCoroutine(FlickerAndDie(lifeBulbs[currentLives]));
                }

                timeRemaining = timePerLetter;
                if (currentLives <= 0)
                {
                    GameOver();
                }
            }
        }
    }

    void UpdateLivesUI()
    {
        if (lifeBulbs.Length == 0) return; 

        for (int i = 0; i < lifeBulbs.Length; i++)
        {
            if (i < currentLives)
            {
                lifeBulbs[i].material = bulbOnMaterial;
            }
            else
            {
                lifeBulbs[i].material = bulbOffMaterial;
            }
        }
    }

    private IEnumerator FlickerAndDie(MeshRenderer bulb)
    {
        if (bulb == null) yield break;

        for (int i = 0; i < 3; i++)
        {
            bulb.material = bulbOffMaterial;
            yield return new WaitForSeconds(0.05f);
            bulb.material = bulbOnMaterial;
            yield return new WaitForSeconds(0.05f);
        }
        
        bulb.material = bulbOffMaterial;
    }

    void GameOver()
    {
        isPlaying = false;
        isTransitioning = true;
        if (clockText != null) clockText.text = "0.0";
        
        gameOverPanel.SetActive(true);
    }

    void NextLetter()
    {
        currentInput = "";

        if (MenuManager.selectedMode == MenuManager.GameMode.Learning)
        {
            currentLetterIndex++;
            if (currentLetterIndex >= alphabet.Count)
            {
                victoryPanel.SetActive(true);
                isPlaying = false;
                return;
            }
        }
        else if (MenuManager.selectedMode == MenuManager.GameMode.VisualChallenge || 
                 MenuManager.selectedMode == MenuManager.GameMode.AudioChallenge)
        {
            if (availableChallengeIndices.Count == 0)
            {
                victoryPanel.SetActive(true);
                isPlaying = false;
                return;
            }

            int randomIndex = Random.Range(0, availableChallengeIndices.Count);
            currentLetterIndex = availableChallengeIndices[randomIndex];
            
            availableChallengeIndices.RemoveAt(randomIndex);

            timeRemaining = timePerLetter; 
        }

        UpdateClipboardUI();
    }

    void UpdateClipboardUI()
    {
        MorseLetter targetLetter = alphabet[currentLetterIndex];
        inputText.text = "";

        AudioClip clipToPlay = null;
        if (MenuManager.selectedDifficulty == MenuManager.Difficulty.Easy) clipToPlay = targetLetter.easyAudio;
        else if (MenuManager.selectedDifficulty == MenuManager.Difficulty.Medium) clipToPlay = targetLetter.mediumAudio;
        else clipToPlay = targetLetter.hardAudio;

        if (MenuManager.selectedMode == MenuManager.GameMode.VisualChallenge)
        {
            letterText.text = targetLetter.letter;
            mnemonicText.text = "";
            sequenceText.text = "";
        }
        else if (MenuManager.selectedMode == MenuManager.GameMode.AudioChallenge)
        {
            letterText.text = "?"; 
            mnemonicText.text = "";
            sequenceText.text = "";

            if (clipToPlay != null && telegraphAudio != null)
            {
                telegraphAudio.clip = clipToPlay;
                telegraphAudio.Play();
                StopAllCoroutines();
                StartCoroutine(FlashLampFromSequence(targetLetter.sequence));
            }
        }
        else 
        {
            letterText.text = targetLetter.letter;
            mnemonicText.text = targetLetter.mnemonic;
            sequenceText.text = targetLetter.sequence;

            if (clipToPlay != null && telegraphAudio != null)
            {
                telegraphAudio.clip = clipToPlay;
                telegraphAudio.Play();
                StopAllCoroutines();
                StartCoroutine(FlashLampFromSequence(targetLetter.sequence));
            }
        }
    }
    
    void PlayErrorSound()
    {
        if (errorAudio != null && errorSound != null)
        {
            errorAudio.Stop();
            errorAudio.clip = errorSound;
            errorAudio.Play();
        }
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenuScene");
    }


    public void PlayTutorialDot()
    {
        if (inputAudio != null && dotSound != null)
        {
            inputAudio.PlayOneShot(dotSound);
        }
    }

    public void PlayTutorialDash()
    {
        if (inputAudio != null && dashSound != null)
        {
            inputAudio.PlayOneShot(dashSound);
        }
    }

    public void CloseTutorial()
    {
        isTutorialActive = false;
        if (tutorialOverlayPanel != null) tutorialOverlayPanel.SetActive(false);
        
        UpdateClipboardUI(); 
    }


    private IEnumerator FlashLampFromSequence(string sequence)
    {
        yield return new WaitForSeconds(0.05f);

        foreach (char symbol in sequence)
        {
            if (symbol == '.')
            {
                lampLight.intensity = dotFlashIntensity;
                currentFadeSpeed = dotFadeSpeed;
                yield return new WaitForSeconds(morseUnitTime);
            }
            else if (symbol == '-')
            {
                lampLight.intensity = dashFlashIntensity;
                currentFadeSpeed = dashFadeSpeed;
                yield return new WaitForSeconds(morseUnitTime * 3f);
            }

            yield return new WaitForSeconds(morseUnitTime);
        }
    }

    private IEnumerator TransitionToNextLetter()
    {
        isTransitioning = true;
        yield return new WaitForSeconds(0.6f);
        lampLight.intensity = 0f;

        NextLetter();

        if (isPlaying)
        {
            isTransitioning = false;
        }
    }

    private IEnumerator ManualFlash(string symbol)
    {
        lampLight.intensity = 0f;
        yield return new WaitForSeconds(0.02f);

        if (symbol == ".")
        {
            lampLight.intensity = dotFlashIntensity;
            currentFadeSpeed = dotFadeSpeed;
        }
        else if (symbol == "-")
        {
            lampLight.intensity = dashFlashIntensity;
            currentFadeSpeed = dashFadeSpeed;
        }
    }
}