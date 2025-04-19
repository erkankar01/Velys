using UnityEngine;
using UnityEngine.UI;

public class UIKontrol : MonoBehaviour
{
    [Header("UI Referansları")]
    [SerializeField] private Canvas anaCanvas;
    [SerializeField] private GameObject silahButonlariParent;
    [SerializeField] private GameObject canBarParent;
    [SerializeField] private GameObject[] silahButonlari;

    private RectTransform silahButonlariRect;
    private RectTransform canBarRect;
    private Button[] butonKomponentleri;

    void Awake()
    {
        // Referansları al
        if (anaCanvas == null) anaCanvas = GetComponent<Canvas>();
        if (silahButonlariParent != null) silahButonlariRect = silahButonlariParent.GetComponent<RectTransform>();
        if (canBarParent != null) canBarRect = canBarParent.GetComponent<RectTransform>();
        
        // Buton komponentlerini al
        if (silahButonlari != null && silahButonlari.Length > 0)
        {
            butonKomponentleri = new Button[silahButonlari.Length];
            for (int i = 0; i < silahButonlari.Length; i++)
            {
                if (silahButonlari[i] != null)
                {
                    butonKomponentleri[i] = silahButonlari[i].GetComponent<Button>();
                }
            }
        }

        // Canvas ayarlarını yap
        CanvasAyarla();
    }

    void Start()
    {
        // UI elemanlarını ayarla
        ButonlariAyarla();
        CanBariAyarla();
    }

    void CanvasAyarla()
    {
        if (anaCanvas != null)
        {
            anaCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = anaCanvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                scaler.matchWidthOrHeight = 0f; // Width'e göre ölçekle
            }

            // Canvas'ın RectTransform'unu ayarla
            RectTransform canvasRect = anaCanvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.sizeDelta = Vector2.zero;
                canvasRect.anchoredPosition = Vector2.zero;
                canvasRect.localScale = Vector3.one;
            }
        }
    }

    void ButonlariAyarla()
    {
        if (silahButonlariRect != null)
        {
            // Silah butonları container'ını ortala
            silahButonlariRect.anchorMin = new Vector2(0.5f, 0.5f);
            silahButonlariRect.anchorMax = new Vector2(0.5f, 0.5f);
            silahButonlariRect.pivot = new Vector2(0.5f, 0.5f);
            silahButonlariRect.anchoredPosition = Vector2.zero;
            silahButonlariRect.sizeDelta = new Vector2(800, 600); // Container boyutu
            silahButonlariRect.localScale = Vector3.one;

            // Butonları düzenle
            if (silahButonlari != null)
            {
                Vector2[] pozisyonlar = new Vector2[]
                {
                    new Vector2(0, 150),      // None
                    new Vector2(-150, 0),     // Rifle
                    new Vector2(150, 0),      // Pistol
                    new Vector2(-150, -150),  // Shotgun
                    new Vector2(150, -150)    // Sniper
                };

                for (int i = 0; i < silahButonlari.Length && i < pozisyonlar.Length; i++)
                {
                    if (silahButonlari[i] != null)
                    {
                        RectTransform rectTransform = silahButonlari[i].GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                            rectTransform.pivot = new Vector2(0.5f, 0.5f);
                            rectTransform.anchoredPosition = pozisyonlar[i];
                            rectTransform.sizeDelta = new Vector2(120, 120); // Buton boyutu
                            rectTransform.localScale = Vector3.one;
                        }
                    }
                }
            }
        }
    }

    void CanBariAyarla()
    {
        if (canBarRect != null)
        {
            // Can barını sağ üst köşeye sabitle
            canBarRect.anchorMin = new Vector2(1, 1);
            canBarRect.anchorMax = new Vector2(1, 1);
            canBarRect.pivot = new Vector2(1, 1);
            canBarRect.anchoredPosition = new Vector2(-20, -20);
            canBarRect.sizeDelta = new Vector2(250, 40); // Can bar boyutu
            canBarRect.localScale = Vector3.one;

            // Can bar'ın child elemanlarını düzelt
            foreach (RectTransform child in canBarRect)
            {
                child.localScale = Vector3.one;
            }
        }
    }

    void OnEnable()
    {
        // UI'ı yeniden ayarla
        CanvasAyarla();
        ButonlariAyarla();
        CanBariAyarla();
    }
} 