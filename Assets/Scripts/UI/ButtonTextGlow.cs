using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonTextGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("References")]
    private TextMeshProUGUI buttonText;

    [Header("Material Presets")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material hoverMaterial;
    [SerializeField] private Material clickedMaterial;

    private bool isHovered = false;

    private void Awake()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnDisable()
    {
        isHovered = false;
        if (buttonText != null) 
        {
            buttonText.fontSharedMaterial = normalMaterial;
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        if(buttonText != null) buttonText.fontSharedMaterial = hoverMaterial;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        if(buttonText != null) buttonText.fontSharedMaterial = normalMaterial;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if(buttonText != null) buttonText.fontSharedMaterial = clickedMaterial;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (buttonText != null) 
        {
            buttonText.fontSharedMaterial = isHovered ? hoverMaterial : normalMaterial;
        }
    }
}