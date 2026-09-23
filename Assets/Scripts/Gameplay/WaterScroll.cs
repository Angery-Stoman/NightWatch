using UnityEngine;

public class WaterScroll : MonoBehaviour
{
    public float scrollSpeed = 0.01f;
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

void Update()
    {
        float offset = Time.time * scrollSpeed;
        
        rend.material.mainTextureOffset = new Vector2(offset, offset);
    }
}