using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 

public class RadarManager : MonoBehaviour
{
    [Header("References")]
    public GameObject drifterPrefab;   
    public GameObject torpedoPrefab;   
    public GameObject leviathanPrefab; 
    public Transform radarCenter;
    public WordBank wordBank;
    public RadarSweep radarSweep; 
    public RadarReticle radarReticle; 

    [Header("UI Panels & Scoring")]
    public GameObject gameOverPanel; 
    public TMP_Text gameOverScoreText; 
    public TMP_Text scoreText; 
    private int currentScore = 0; 

    [Header("Life UI (3D LEDs)")]
    public GameObject[] lifeLEDs; 
    public Material ledOnMaterial;  
    public Material ledOffMaterial; 

    [Header("Visual Juice (Juice & Effects)")]
    public ParticleSystem explosionPrefab; 
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 0.4f;
    [ColorUsage(true, true)] public Color drifterColor = Color.green;
    [ColorUsage(true, true)] public Color torpedoColor = Color.red;
    [ColorUsage(true, true)] public Color leviathanColor = new Color(0.6f, 0.1f, 0.9f); 

    [Header("Audio")]
    public AudioSource sfxSource;
    public AudioClip destroySound;
    public AudioClip damageSound;

    [Header("Spawning & Difficulty")]
    public float radarRadius = 10f; 
    public float initialSpawnTime = 4f; 
    public float minimumSpawnTime = 1.2f; 
    public float difficultyMultiplier = 0.95f; 

    [Header("Game State")]
    public int maxLives = 3;
    private int currentLives;
    private float currentSpawnTime;
    private float spawnTimer;
    private bool isGameOver = false;

    public List<RadarBlip> activeBlips = new List<RadarBlip>();

    void Start()
    {
        currentLives = maxLives;
        currentSpawnTime = initialSpawnTime;
        spawnTimer = 2f; 
        currentScore = 0;

        UpdateScoreDisplay(); 

        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        foreach (GameObject led in lifeLEDs)
        {
            if (led != null) SetLEDMaterial(led, ledOnMaterial);
        }
    }

    void Update()
    {
        if (isGameOver) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0)
        {
            SpawnBlip();
            currentSpawnTime = Mathf.Max(minimumSpawnTime, currentSpawnTime * difficultyMultiplier);
            spawnTimer = currentSpawnTime;
        }
    }

    void SpawnBlip()
    {
        float rand = Random.value;
        string enemyType = "Drifter";
        float speed = 0.25f;
        GameObject prefabToSpawn = drifterPrefab; 

        if (rand < 0.15f) 
        {
            enemyType = "Torpedo";
            speed = 0.5f; 
            prefabToSpawn = torpedoPrefab;
        }
        else if (rand < 0.25f) 
        {
            enemyType = "Leviathan";
            speed = 0.125f; 
            prefabToSpawn = leviathanPrefab;
        }

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = radarCenter.position + new Vector3(randomDir.x, 0, randomDir.y) * radarRadius;

        GameObject newBlipObj = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        RadarBlip newBlip = newBlipObj.GetComponent<RadarBlip>();

        string word = wordBank.GetRandomWord(enemyType);
        newBlip.Setup(word, radarCenter, speed, this);
        
        activeBlips.Add(newBlip);
    }

    public void ReportDamage(RadarBlip blip)
    {
        activeBlips.Remove(blip);
        Destroy(blip.gameObject);
        
        currentLives--;

        if (sfxSource != null && damageSound != null)
        {
            sfxSource.PlayOneShot(damageSound);
        }

        StartCoroutine(ShakeCamera(shakeDuration, shakeMagnitude));

        if (currentLives >= 0 && currentLives < lifeLEDs.Length)
        {
            StartCoroutine(FlickerAndDie(lifeLEDs[currentLives]));
        }

        if (currentLives <= 0) 
        {
            TriggerGameOver();
        }
    }

    public void ReportDestroyed(RadarBlip blip, int points)
    {
        if (sfxSource != null && destroySound != null)
        {
            sfxSource.PlayOneShot(destroySound);
        }

        if (explosionPrefab != null)
        {
            ParticleSystem explosion = Instantiate(explosionPrefab, blip.transform.position, Quaternion.identity);
            var main = explosion.main;
            
            if (points == 10) main.startColor = drifterColor;
            else if (points == 15) main.startColor = torpedoColor;
            else if (points >= 20) main.startColor = leviathanColor;
            
            Destroy(explosion.gameObject, main.duration); 
        }

        activeBlips.Remove(blip);
        Destroy(blip.gameObject);

        currentScore += points;
        UpdateScoreDisplay();
    }

    private IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude; 

            Camera.main.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y, originalPos.z + z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString("D5"); 
        }
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

        if (radarSweep != null) radarSweep.enabled = false; 
        if (radarReticle != null) radarReticle.enabled = false;

        foreach (RadarBlip blip in activeBlips)
        {
            if (blip != null) Destroy(blip.gameObject);
        }
        activeBlips.Clear(); 

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "FINAL SCORE: " + currentScore.ToString("D5");
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}