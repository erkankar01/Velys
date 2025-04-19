using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace GeeZyyGames.ShootingKit
{
    public class ParaYonetimi : MonoBehaviour
    {
        public static ParaYonetimi Instance { get; private set; }

        [Header("UI Elemanları")]
        public TextMeshProUGUI paraText;
        public Image paraIkonu;

        [Header("UI Ayarları")]
        [Range(0f, 1f)]
        public float uiSaydamlik = 0.8f; // 0.8 = %80 opaklık

        [Header("Para Ayarları")]
        public int baslangicParasi = 0;
        public int canavarOldurmeOdulu = 50;

        private int mevcutPara;
        private Color normalRenk;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            mevcutPara = baslangicParasi;
            
            // UI elemanlarının pozisyonlarını sabitle
            if (paraText != null)
            {
                RectTransform rectTransform = paraText.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Sağ üst köşeye sabitle
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    rectTransform.anchoredPosition = new Vector2(-200, -30); // Pozisyonu ayarlayın
                    rectTransform.sizeDelta = new Vector2(200, 50); // Boyutu ayarlayın
                }
                
                normalRenk = new Color(1f, 1f, 1f, uiSaydamlik);
                paraText.color = normalRenk;
                paraText.raycastTarget = false;
            }
            
            if (paraIkonu != null)
            {
                RectTransform rectTransform = paraIkonu.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    // Sağ üst köşeye sabitle
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.pivot = new Vector2(1, 1);
                    rectTransform.anchoredPosition = new Vector2(-250, -25); // Pozisyonu ayarlayın
                    rectTransform.sizeDelta = new Vector2(40, 40); // Boyutu ayarlayın
                }
                
                Color ikonRenk = paraIkonu.color;
                ikonRenk.a = uiSaydamlik;
                paraIkonu.color = ikonRenk;
                paraIkonu.raycastTarget = false;
            }

            // Canvas ölçeklendirmesini sabitle
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    scaler.scaleFactor = 1;
                }
            }
            
            ParayiGuncelle();
        }

        public void ParaEkle(int miktar)
        {
            mevcutPara += miktar;
            ParayiGuncelle();
            
            // Para kazanma efekti
            if (paraText != null)
            {
                StartCoroutine(ParaEfekti());
            }
        }

        public bool ParaHarca(int miktar)
        {
            if (mevcutPara >= miktar)
            {
                mevcutPara -= miktar;
                ParayiGuncelle();
                return true;
            }
            return false;
        }

        public int MevcutParayiAl()
        {
            return mevcutPara;
        }

        private void ParayiGuncelle()
        {
            if (paraText != null)
            {
                // Para miktarını formatlı göster (örn: "1,234")
                paraText.text = string.Format("{0:N0}", mevcutPara);
            }
        }

        private System.Collections.IEnumerator ParaEfekti()
        {
            if (paraText != null)
            {
                // Para kazanıldığında efekt rengi (sarı, yarı saydam)
                Color efektRenk = new Color(1f, 1f, 0f, uiSaydamlik);
                paraText.color = efektRenk;
                paraText.fontSize += 2;

                yield return new WaitForSeconds(0.1f);

                paraText.color = normalRenk;
                paraText.fontSize -= 2;
            }
        }
    }
} 