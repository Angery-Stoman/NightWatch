using UnityEngine;

public class LightbulbSway : MonoBehaviour
{
    [Header("Rotation")]
    public bool enableRotationSway = true;
    public float swayAngle = 10f; 
    public float swaySpeed = 1.5f;
    public Vector3 swingAxis = Vector3.forward;

    [Header("Position")]
    public bool enableYBobbing = false;
    public float bobHeight = 0.2f;
    public float bobSpeed = 1f;

    private Quaternion startRotation;
    private Vector3 startPosition;

    void Start()
    {

        startRotation = transform.localRotation;
        startPosition = transform.localPosition;
    }

    void Update()
    {
        if (enableRotationSway)
        {

            float currentAngle = Mathf.Sin(Time.time * swaySpeed) * swayAngle;
            transform.localRotation = startRotation * Quaternion.AngleAxis(currentAngle, swingAxis);
        }

        if (enableYBobbing)
        {
            float newY = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.localPosition = startPosition + new Vector3(0, newY, 0);
        }
    }
}