using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class RadarReticle : MonoBehaviour
{
    [Header("References")]
    public RadarManager radarManager; 
    public Transform radarCenter;
    
    [Header("Visuals")]
    public Renderer[] reticleRenderers; 
    public Color normalColor = Color.green;
    public Color lockedColor = Color.red;

    [Header("Movement & Targeting")]
    public float moveSpeed = 15f;
    public float maxRadarRadius = 10f; 
    public float lockOnDistance = 1.5f; 

    [Header("UI")]
    public TMP_Text morseInputDisplay;
    
    private RadarBlip currentTarget = null;
    private bool isLockedOn = false;

    void Start()
    {
        foreach (Renderer r in reticleRenderers)
        {
            if (r != null) r.material.color = normalColor;
        }
    }

    void Update()
    {   
        if (isLockedOn && currentTarget == null)
        {
            BreakLock();
        }

        if (morseInputDisplay != null)
        {
            if (isLockedOn && currentTarget != null)
            {
                morseInputDisplay.text = currentTarget.GetAcceptedMorse();
            }
            else
            {
                morseInputDisplay.text = ""; 
            }
        }
        HandleMovement();
        HandleTargeting();
        HandleMorseInput();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        
        Vector3 moveInput = new Vector3(horizontal, 0f, vertical).normalized;

        if (isLockedOn && moveInput.sqrMagnitude > 0)
        {
            BreakLock();
        }

        if (!isLockedOn)
        {
            transform.position += moveInput * moveSpeed * Time.deltaTime;

            Vector2 flatReticlePos = new Vector2(transform.position.x, transform.position.z);
            Vector2 flatCenterPos = new Vector2(radarCenter.position.x, radarCenter.position.z);

            float flatDistance = Vector2.Distance(flatReticlePos, flatCenterPos);

            if (flatDistance > maxRadarRadius)
            {
                Vector2 fromCenter = (flatReticlePos - flatCenterPos).normalized;
                
                transform.position = new Vector3(
                    radarCenter.position.x + (fromCenter.x * maxRadarRadius),
                    transform.position.y, 
                    radarCenter.position.z + (fromCenter.y * maxRadarRadius) 
                );
            }
        }
        else if (currentTarget != null)
        {
            transform.position = currentTarget.transform.position;
        }
    }

    void HandleTargeting()
    {
        if (isLockedOn) return;

        foreach (RadarBlip blip in radarManager.activeBlips)
        {
            if (blip == null) continue; 

            float distance = Vector3.Distance(transform.position, blip.transform.position);
            
            if (distance <= lockOnDistance)
            {
                LockOnTo(blip);
                break; 
            }
        }
    }

    void HandleMorseInput()
    {
        if (!isLockedOn || currentTarget == null) return;

        if (Input.GetKeyDown(KeyCode.Period))
        {
            currentTarget.ReceiveInput(".");
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            currentTarget.ReceiveInput("-");
        }
    }

    void LockOnTo(RadarBlip target)
    {
        isLockedOn = true;
        currentTarget = target;
        currentTarget.isTargeted = true;
        currentTarget.PingRadar(); 
        
        foreach (Renderer r in reticleRenderers)
        {
            if (r != null) r.material.color = lockedColor;
        }
    }

    void BreakLock()
    {
        if (currentTarget != null) 
        {
            currentTarget.isTargeted = false;
        }

        isLockedOn = false;
        currentTarget = null;
        
        foreach (Renderer r in reticleRenderers)
        {
            if (r != null) r.material.color = normalColor;
        }
    }
    void OnDrawGizmos()
    {
        if (radarCenter != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(radarCenter.position, maxRadarRadius);
        }
    }
}