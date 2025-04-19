using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

namespace GeeZyyGames.ShootingKit
{
    public class ParaYonetimi : MonoBehaviour
    {
        private static ParaYonetimi instance;
        public static ParaYonetimi Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ParaYonetimi>();
                }
                return instance;
            }
        }

        [Header("UI Elemanları")]
        [SerializeField] private TextMeshProUGUI paraText;
        [SerializeField] private Image paraImage;

        [Header("UI Ayarları")]
        [Range(0f, 1f)]
        public float uiSaydamlik = 0.8f; // 0.8 = %80 opaklık

        [Header("Para Ayarları")]
        public int baslangicParasi = 0;
        public int canavarOldurmeOdulu = 50;

        private int toplamPara;
        private Color normalRenk;

        private void Awake()
        {
            instance = this;
        }

        void Start()
        {
            if (paraText == null)
            {
                paraText = transform.Find("moneyText")?.GetComponent<TextMeshProUGUI>();
            }
            if (paraImage == null)
            {
                paraImage = transform.Find("moneyIcon")?.GetComponent<Image>();
            }

            toplamPara = baslangicParasi;
            normalRenk = Color.white;
            
            if (paraText != null)
            {
                paraText.raycastTarget = false;
            }
            
            if (paraImage != null)
            {
                paraImage.raycastTarget = false;
            }
            
            ParayiGuncelle();
        }

        public void ParaEkle(int miktar)
        {
            toplamPara += miktar;
            ParayiGuncelle();
            ParaEfektiOynat();
        }

        public bool ParaHarca(int miktar)
        {
            if (toplamPara >= miktar)
            {
                toplamPara -= miktar;
                ParayiGuncelle();
                return true;
            }
            return false;
        }

        public int ToplamParayiAl()
        {
            return toplamPara;
        }

        private void ParayiGuncelle()
        {
            if (paraText != null)
            {
                paraText.text = toplamPara.ToString();
            }
        }

        private void ParaEfektiOynat()
        {
            if (paraText != null)
            {
                StartCoroutine(ParaEfektiCoroutine());
            }
        }

        private IEnumerator ParaEfektiCoroutine()
        {
            if (paraText != null)
            {
                // Efekt başlangıcı
                paraText.color = Color.yellow;
                paraText.transform.localScale = Vector3.one * 1.2f;

                yield return new WaitForSeconds(0.2f);

                // Efekt sonu
                paraText.color = normalRenk;
                paraText.transform.localScale = Vector3.one;
            }
        }
    }
} 