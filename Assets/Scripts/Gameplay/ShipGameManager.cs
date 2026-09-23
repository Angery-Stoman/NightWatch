using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement; 

public class ShipGameManager : MonoBehaviour
{
    [Header("Ship 1: Small (Close)")]
    public GameObject smallShip;
    public Light smallLight; 
    public Light smallLamp;  
    public Transform[] smallSpawns;

    [Header("Ship 2: Medium (Mid)")]
    public GameObject mediumShip;
    public Light mediumLight;
    public Light mediumLamp; 
    public Transform[] mediumSpawns;

    [Header("Ship 3: Large (Far)")]
    public GameObject largeShip;
    public Light largeLight;
    public Light largeLamp;  
    public Transform[] largeSpawns;

    [Header("Active Target Tracking")]
    private GameObject currentActiveShip;
    private Light currentActiveLight;
    private Light currentActiveLamp; 
    private Transform lastSpawnPoint; 

    [Header("UI & Game Rules")]
    public TextMeshProUGUI inputText;
    public GameObject gameOverPanel;         
    public TextMeshProUGUI finalScoreText;   
    private int currentScore = 0;            

    public int maxLives = 3;
    private int currentLives;
    public float timePerLetter = 10f;
    private float timeRemaining;
    private bool isPlaying = false;
    private bool isTransitioning = false;

    [Header("Game Over Sequence")]
    public Transform playerBoat;             
    public MonoBehaviour cameraController;   
    public ParticleSystem explosionParticles;
    public ParticleSystem flashParticles;    
    public AudioSource explosionAudio;       
    public float capsizeDuration = 3.5f;    

    [Header("Underwater Effects")]
    public Color underwaterColor = new Color(0.02f, 0.08f, 0.15f); 
    public float underwaterFogDensity = 0.55f;
    public ParticleSystem bubbleParticles; 
    
    private Color normalFogColor;
    private float normalFogDensity;
    private CameraClearFlags normalClearFlags;

    [Header("Audio & Motion")]
    public AudioSource foghornAudio;
    public AudioSource enemyHumAudio;
    public AudioSource waterDisplacementAudio;
    public AudioClip[] waterDisplacementClips;
    public AudioSource shipBellAudio;
    public AudioClip[] shipBellClips;
    public AudioSource tickingClockAudio;
    public AudioSource mistakeStingerAudio;
    public AudioSource ambientWaterAudio;
    public Transform enemyAudioTracker;
    
    public float swayAmount = 1.5f;   
    public float swaySpeed = 1f;      
    public float bobAmount = 0.1f;    
    private Vector3 baseBoatPos;
    private Quaternion baseBoatRot;

    [Header("Morse Flash Settings")]
    public float flashIntensity = 10f; 
    public float turnOnSpeed = 40f;    
    public float dotFadeSpeed = 25f;   
    public float dashFadeSpeed = 5f;   
    public float morseUnitTime = 0.3f; 

    [Header("Fog Emergence Settings")]
    public float shipGlideSpeed = 5f;  
    public float fogDepthOffset = 5f; 
    private Vector3 targetPosition;    

    private float targetIntensity = 0f;
    private float currentFadeSpeed = 15f;
    private Coroutine currentFlashCoroutine; 

    [Header("Data")]
    public List<MorseLetter> alphabet; 
    private int currentLetterIndex;

    void Start()
    {
        normalFogColor = RenderSettings.fogColor;
        normalFogDensity = RenderSettings.fogDensity;
        if (Camera.main != null) normalClearFlags = Camera.main.clearFlags;

        currentLives = maxLives;
        currentScore = 0; 
        isPlaying = true;

        if (gameOverPanel != null) gameOverPanel.SetActive(false); 

        smallShip.SetActive(false);
        mediumShip.SetActive(false);
        largeShip.SetActive(false);

        if (playerBoat != null)
        {
            baseBoatPos = playerBoat.position;
            baseBoatRot = playerBoat.rotation;
        }

        SpawnNextShip();
        
        StartCoroutine(FoghornRoutine());
    }

    void Update()
    {
        if (isPlaying && playerBoat != null)
        {
            float rockX = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
            float rockZ = Mathf.Cos(Time.time * swaySpeed * 0.8f) * (swayAmount * 0.5f);
            float bobY = Mathf.Sin(Time.time * swaySpeed * 1.2f) * bobAmount;

            playerBoat.rotation = baseBoatRot * Quaternion.Euler(rockX, 0, rockZ);
            playerBoat.position = baseBoatPos + new Vector3(0, bobY, 0);
        }

        if (currentActiveShip != null && currentActiveShip.activeSelf)
        {
            if (currentActiveLight != null)
            {
                currentActiveLight.transform.LookAt(Camera.main.transform.position);
            }

            currentActiveShip.transform.position = Vector3.MoveTowards(
                currentActiveShip.transform.position, 
                targetPosition, 
                Time.deltaTime * shipGlideSpeed
            );
            
            if (enemyAudioTracker != null)
            {
                enemyAudioTracker.position = currentActiveShip.transform.position;
            }
        }

        if (currentActiveLight != null)
        {
            currentActiveLight.intensity = Mathf.Lerp(currentActiveLight.intensity, targetIntensity, Time.deltaTime * currentFadeSpeed);
        }
        if (currentActiveLamp != null)
        {
            currentActiveLamp.intensity = Mathf.Lerp(currentActiveLamp.intensity, targetIntensity, Time.deltaTime * currentFadeSpeed);
        }

        if (isPlaying && !isTransitioning)
        {
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 8f)
            {
                if (tickingClockAudio != null && !tickingClockAudio.isPlaying) tickingClockAudio.Play();
            }
            else
            {
                if (tickingClockAudio != null && tickingClockAudio.isPlaying) tickingClockAudio.Stop();
            }

            if (timeRemaining <= 0)
            {
                Debug.Log("Time's Up! You die!");
                GameOver(); 
            }

            HandleTranslationInput();
        }
    }

    private IEnumerator FoghornRoutine()
    {
        while (isPlaying)
        {
            yield return new WaitForSeconds(Random.Range(40f, 55f));
            if (isPlaying && foghornAudio != null) foghornAudio.Play();
        }
    }

    void HandleTranslationInput()
    {
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
        if (isTransitioning) return;

        string targetLetter = alphabet[currentLetterIndex].letter;
        if (inputText != null) inputText.text = typedLetter;

        if (typedLetter == targetLetter)
        {
            Debug.Log("Correct! Enemy ship identified.");
            currentScore++; 
            StartCoroutine(TransitionToNextShip());
        }
        else
        {
            Debug.Log("Mistake! Wrong letter.");
            ProcessMistake();
        }
    }

    void ProcessMistake()
    {
        if (mistakeStingerAudio != null) mistakeStingerAudio.Play();
        
        currentLives--;
        timeRemaining = timePerLetter; 

        if (currentLives <= 0 && isPlaying)
        {
            GameOver(); 
        }
    }

    void GameOver()
    {
        isPlaying = false; 
        targetIntensity = 0f;
        
        if (tickingClockAudio != null) tickingClockAudio.Stop();
        if (enemyHumAudio != null) enemyHumAudio.Stop();
        if (foghornAudio != null) foghornAudio.Stop();
        if (shipBellAudio != null) shipBellAudio.Stop();
        if (waterDisplacementAudio != null) waterDisplacementAudio.Stop();
        if (ambientWaterAudio != null) ambientWaterAudio.Stop();
        if (tickingClockAudio != null) tickingClockAudio.Stop();
        if (enemyHumAudio != null) enemyHumAudio.Stop();

        if (currentActiveShip != null)
        {
            Vector3 directionAwayFromCamera = (currentActiveShip.transform.position - Camera.main.transform.position).normalized;
            targetPosition = currentActiveShip.transform.position + (directionAwayFromCamera * fogDepthOffset);
        }

        StartCoroutine(GameOverCinematic());
    }

  private IEnumerator GameOverCinematic()
    {
        if (cameraController != null) cameraController.enabled = false;

        if (explosionParticles != null) explosionParticles.Play();
        if (flashParticles != null) flashParticles.Play(); 
        if (explosionAudio != null) explosionAudio.Play();

        if (playerBoat != null)
        {
            Quaternion startRot = playerBoat.rotation;
            Vector3 startPos = playerBoat.position;

            Quaternion lurchRot = startRot * Quaternion.Euler(0, 0, 25f);   
            Quaternion capsizeRot = startRot * Quaternion.Euler(0, 0, -110f); 
            Vector3 endPos = startPos + new Vector3(0, -4f, 0); 

            float phase1Duration = capsizeDuration * 0.25f;
            float elapsed = 0f;

            while (elapsed < phase1Duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / phase1Duration;
                float ease = Mathf.SmoothStep(0f, 1f, t); 

                playerBoat.rotation = Quaternion.Slerp(startRot, lurchRot, ease);
                playerBoat.position = Vector3.Lerp(startPos, startPos + new Vector3(0, -0.5f, 0), ease); 
                
                yield return null;
            }

            float phase2Duration = capsizeDuration * 0.75f;
            elapsed = 0f;
            Vector3 midPos = playerBoat.position; 

            if (Camera.main != null) Camera.main.clearFlags = CameraClearFlags.SolidColor;
            
            if (bubbleParticles != null) bubbleParticles.Play();

            while (elapsed < phase2Duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / phase2Duration;
                float ease = Mathf.SmoothStep(0f, 1f, t);

                playerBoat.rotation = Quaternion.Slerp(lurchRot, capsizeRot, ease);
                playerBoat.position = Vector3.Lerp(midPos, endPos, ease);
                
                RenderSettings.fogColor = Color.Lerp(normalFogColor, underwaterColor, ease);
                RenderSettings.fogDensity = Mathf.Lerp(normalFogDensity, underwaterFogDensity, ease);
                if (Camera.main != null) Camera.main.backgroundColor = RenderSettings.fogColor;
                
                yield return null;
            }
        }
        else 
        {
            yield return new WaitForSeconds(capsizeDuration); 
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (finalScoreText != null) finalScoreText.text = "Ships Spotted: " + currentScore;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void SpawnNextShip()
    {
        if (currentActiveShip != null) currentActiveShip.SetActive(false);

        if (shipBellAudio != null && shipBellClips.Length > 0)
        {
            int randomBell = Random.Range(0, shipBellClips.Length);
            shipBellAudio.clip = shipBellClips[randomBell];
            shipBellAudio.Play();
        }

        if (waterDisplacementAudio != null && waterDisplacementClips.Length > 0)
        {
            int randomSplash = Random.Range(0, waterDisplacementClips.Length);
            waterDisplacementAudio.clip = waterDisplacementClips[randomSplash];
            waterDisplacementAudio.Play();
        }

        currentLetterIndex = Random.Range(0, alphabet.Count);
        int shipType = Random.Range(0, 3);
        
        Transform chosenSpawn = null; 

        if (shipType == 0) 
        {
            currentActiveShip = smallShip;
            currentActiveLight = smallLight;
            currentActiveLamp = smallLamp; 
            do { chosenSpawn = smallSpawns[Random.Range(0, smallSpawns.Length)]; } while (chosenSpawn == lastSpawnPoint);
        }
        else if (shipType == 1) 
        {
            currentActiveShip = mediumShip;
            currentActiveLight = mediumLight;
            currentActiveLamp = mediumLamp; 
            do { chosenSpawn = mediumSpawns[Random.Range(0, mediumSpawns.Length)]; } while (chosenSpawn == lastSpawnPoint);
        }
        else if (shipType == 2) 
        {
            currentActiveShip = largeShip;
            currentActiveLight = largeLight;
            currentActiveLamp = largeLamp; 
            do { chosenSpawn = largeSpawns[Random.Range(0, largeSpawns.Length)]; } while (chosenSpawn == lastSpawnPoint);
        }

        lastSpawnPoint = chosenSpawn;

        Vector3 directionAwayFromCamera = (chosenSpawn.position - Camera.main.transform.position).normalized;
        Vector3 deepFogPosition = chosenSpawn.position + (directionAwayFromCamera * fogDepthOffset);

        currentActiveShip.transform.position = deepFogPosition;
        targetPosition = chosenSpawn.position;
        currentActiveShip.SetActive(true);
        
        targetIntensity = 0f;
        if (currentActiveLight != null) currentActiveLight.intensity = 0f;
        if (currentActiveLamp != null) currentActiveLamp.intensity = 0f;

        timeRemaining = timePerLetter;
        if (inputText != null) inputText.text = ""; 
        
        if (currentFlashCoroutine != null) StopCoroutine(currentFlashCoroutine);
        currentFlashCoroutine = StartCoroutine(FlashLampFromSequence(alphabet[currentLetterIndex].sequence));
    }

    private IEnumerator FlashLampFromSequence(string sequence)
    {
        yield return new WaitForSeconds(1.0f);

        foreach (char symbol in sequence)
        {
            if (isTransitioning) yield break; 

            targetIntensity = flashIntensity;
            currentFadeSpeed = turnOnSpeed;
            if (enemyHumAudio != null) enemyHumAudio.Play(); 

            if (symbol == '.')
            {
                yield return new WaitForSeconds(morseUnitTime); 
                targetIntensity = 0f;
                currentFadeSpeed = dotFadeSpeed; 
                if (enemyHumAudio != null) enemyHumAudio.Stop(); 
            }
            else if (symbol == '-')
            {
                yield return new WaitForSeconds(morseUnitTime * 3f); 
                targetIntensity = 0f;
                currentFadeSpeed = dashFadeSpeed;
                if (enemyHumAudio != null) enemyHumAudio.Stop(); 
            }

            yield return new WaitForSeconds(morseUnitTime); 
        }

        if (!isTransitioning && isPlaying)
        {
            yield return new WaitForSeconds(2.0f);
            currentFlashCoroutine = StartCoroutine(FlashLampFromSequence(sequence));
        }
    }

    private IEnumerator TransitionToNextShip()
    {
        isTransitioning = true;
        if (currentFlashCoroutine != null) StopCoroutine(currentFlashCoroutine);

        if (tickingClockAudio != null) tickingClockAudio.Stop();
        if (enemyHumAudio != null) enemyHumAudio.Stop();

        targetIntensity = 0f;
        currentFadeSpeed = dashFadeSpeed;
        
        Vector3 directionAwayFromCamera = (currentActiveShip.transform.position - Camera.main.transform.position).normalized;
        targetPosition = currentActiveShip.transform.position + (directionAwayFromCamera * fogDepthOffset);
        
        yield return new WaitForSeconds(1.2f); 
        SpawnNextShip();
        isTransitioning = false;
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