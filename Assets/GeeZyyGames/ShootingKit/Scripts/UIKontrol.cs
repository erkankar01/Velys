using UnityEngine;
using UnityEngine.UI;

public class UIKontrol : MonoBehaviour
{
    [SerializeField] private Canvas anaCanvas;

    void Start()
    {
        if (anaCanvas == null) 
        {
            anaCanvas = GetComponent<Canvas>();
        }
        
        if (anaCanvas != null)
        {
            anaCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = anaCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            }
        }
    }
} 