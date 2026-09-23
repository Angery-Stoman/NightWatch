using System.Collections;
using UnityEngine;

public class MainMenuLampEffect : MonoBehaviour
{
    [Header("Lamp Components")]
    public Light lampLight;
    public MeshRenderer lampBulbRenderer;
    public Material lampBulbOnMaterial;
    public Material lampBulbOffMaterial;
    public int bulbMaterialIndex = 6;

    [Header("Flash Settings (12 WPM)")]
    public float dotIntensity = 1f;
    public float dashIntensity = 5f;
    public float fadeSpeed = 10f;

    private float morseUnitTime = 0.1f; 
    private float targetIntensity = 0f;
    private bool isFlashingActive = false;
    private bool isBulbOn = false;

    private readonly string[] morseAlphabet = {
        ".-", "-...", "-.-.", "-..", ".", "..-.", "--.", "....", "..", ".---",
        "-.-", ".-..", "--", "-.", "---", ".--.", "--.-", ".-.", "...", "-",
        "..-", "...-", ".--", "-..-", "-.--", "--.."
    };

    void Start()
    {
        StartFlashing();
    }

    void Update()
    {
        if (lampLight != null)
        {
            lampLight.intensity = Mathf.Lerp(lampLight.intensity, targetIntensity, Time.deltaTime * fadeSpeed);

            if (lampBulbRenderer != null)
            {
                if (lampLight.intensity > 0.1f && !isBulbOn)
                {
                    Material[] mats = lampBulbRenderer.materials;
                    mats[bulbMaterialIndex] = lampBulbOnMaterial;
                    lampBulbRenderer.materials = mats;
                    isBulbOn = true;
                }
                else if (lampLight.intensity <= 0.1f && isBulbOn)
                {
                    Material[] mats = lampBulbRenderer.materials;
                    mats[bulbMaterialIndex] = lampBulbOffMaterial;
                    lampBulbRenderer.materials = mats;
                    isBulbOn = false;
                }
            }
        }
    }

    private IEnumerator RandomFlashRoutine()
    {
        yield return new WaitForSeconds(Random.Range(2f, 4f));

        while (isFlashingActive)
        {
            int lettersToPlay = Random.Range(1, 4);

            for (int i = 0; i < lettersToPlay; i++)
            {
                if (!isFlashingActive) yield break; 

                string randomSequence = morseAlphabet[Random.Range(0, morseAlphabet.Length)];
                yield return StartCoroutine(PlaySequence(randomSequence));

                yield return new WaitForSeconds(morseUnitTime * 3f);
            }

            if (isFlashingActive)
            {
                yield return new WaitForSeconds(Random.Range(5f, 10f));
            }
        }
    }

    private IEnumerator PlaySequence(string sequence)
    {
        foreach (char symbol in sequence)
        {
            if (!isFlashingActive) yield break;

            if (symbol == '.')
            {
                targetIntensity = dotIntensity;
                yield return new WaitForSeconds(morseUnitTime);
            }
            else if (symbol == '-')
            {
                targetIntensity = dashIntensity;
                yield return new WaitForSeconds(morseUnitTime * 3f); 
            }

            targetIntensity = 0f;
            yield return new WaitForSeconds(morseUnitTime);
        }
    }


    public void StopFlashing()
    {
        if (!isFlashingActive) return;

        isFlashingActive = false;
        targetIntensity = 0f;
        StopAllCoroutines(); 
        
        if (lampLight != null) lampLight.intensity = 0f;
        if (lampBulbRenderer != null && isBulbOn)
        {
            Material[] mats = lampBulbRenderer.materials;
            mats[bulbMaterialIndex] = lampBulbOffMaterial;
            lampBulbRenderer.materials = mats;
            isBulbOn = false;
        }
    }

    public void StartFlashing()
    {
        if (!isFlashingActive)
        {
            isFlashingActive = true;
            StartCoroutine(RandomFlashRoutine());
        }
    }
}