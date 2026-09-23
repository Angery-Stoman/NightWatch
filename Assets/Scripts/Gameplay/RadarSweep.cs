using UnityEngine;

public class RadarSweep : MonoBehaviour
{
    public float rotationSpeed = 60f; 

    [Header("Audio")]
    public AudioSource sweepAudioSource;
    public AudioClip pingSound;

    void Update()
    {
        if (transform.parent != null)
        {
            transform.parent.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        RadarBlip blip = other.GetComponent<RadarBlip>();
        if (blip != null)
        {
            blip.PingRadar(); 
            
            if (sweepAudioSource != null && pingSound != null)
            {
                sweepAudioSource.PlayOneShot(pingSound);
            }
        }
    }
}