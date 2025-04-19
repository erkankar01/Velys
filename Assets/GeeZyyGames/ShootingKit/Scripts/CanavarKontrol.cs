using UnityEngine;
using System.Collections;

namespace GeeZyyGames.ShootingKit
{
    public class CanavarKontrol : MonoBehaviour
    {
        private Animator animator;
        private Transform player;
        private CharacterController controller;
        private AudioSource audioSource;
        private bool isDead = false;
        
        [Header("Düşman Ayarları")]
        public float algilama_mesafesi = 20f;
        public float saldiri_mesafesi = 2f;
        public float hareket_hizi = 5f;
        public float saldiri_hasari = 15f;
        public float saldiri_gecikmesi = 2f;
        private float son_saldiri_zamani;
        private bool oyuncu_algilandi = false;

        [Header("Ses Ayarları")]
        public AudioClip hasarSesi;
        public AudioClip olumSesi;
        [Range(0f, 1f)]
        public float sesVolumu = 0.7f;

        [Header("Can Ayarları")]
        public float maksimum_can = 100f;
        private float mevcut_can;

        [Header("Silah Hasar Ayarları")]
        public float tabanca_hasari = 20f;
        public float tufek_hasari = 35f;
        public float keskin_nisanci_hasari = 50f;
        public float pompali_hasari = 45f;

        private float yercekimi = -9.81f;
        private float dikey_hiz;

        private readonly int runHash = Animator.StringToHash("Run");
        private readonly int attackHash = Animator.StringToHash("Attack");
        private readonly int deathHash = Animator.StringToHash("Death");
        private readonly int takeDamageHash = Animator.StringToHash("TakeDamage");

        void Start()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<CharacterController>();
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            mevcut_can = maksimum_can;

            SetupAudioSource();

            if (controller == null)
            {
                controller = gameObject.AddComponent<CharacterController>();
                controller.center = new Vector3(0, 1, 0);
                controller.height = 2f;
                controller.radius = 0.5f;
            }
        }

        void SetupAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 2f;
            audioSource.maxDistance = 30f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
            audioSource.volume = sesVolumu;
            audioSource.dopplerLevel = 0f;
            audioSource.spread = 90f;
        }

        public void HasarAl(string silahTipi)
        {
            if (isDead) return;

            int hasar = 0;
            switch (silahTipi.ToLower())
            {
                case "pistol":
                    hasar = (int)tabanca_hasari;
                    break;
                case "rifle":
                    hasar = (int)tufek_hasari;
                    break;
                case "sniper":
                    hasar = (int)keskin_nisanci_hasari;
                    break;
                case "shotgun":
                    hasar = (int)pompali_hasari;
                    break;
                default:
                    hasar = 20;
                    break;
            }

            mevcut_can -= hasar;
            oyuncu_algilandi = true;

            // Hasar sesi çal
            if (hasarSesi != null && audioSource != null)
            {
                audioSource.pitch = Random.Range(0.95f, 1.05f);
                audioSource.PlayOneShot(hasarSesi, sesVolumu);
            }

            // Hasar animasyonunu tetikle
            animator.SetTrigger(takeDamageHash);

            // Canı 0'a eşit veya altına düştüyse öldür
            if (mevcut_can <= 0)
            {
                mevcut_can = 0; // Negatif değerleri engelle
                if (!isDead)
                {
                    OlumAnimasyonunuOynat();
                }
            }
        }

        void Update()
        {
            if (player == null || isDead || mevcut_can <= 0) return;

            // Yerçekimi uygula
            if (!controller.isGrounded)
            {
                dikey_hiz += yercekimi * Time.deltaTime;
            }
            else
            {
                dikey_hiz = -1f;
            }

            float mesafe = Vector3.Distance(transform.position, player.position);

            if (mesafe <= algilama_mesafesi || oyuncu_algilandi)
            {
                oyuncu_algilandi = true;

                Vector3 yon = (player.position - transform.position).normalized;
                yon.y = 0;
                
                Quaternion hedefRotasyon = Quaternion.LookRotation(yon);
                transform.rotation = Quaternion.Slerp(transform.rotation, hedefRotasyon, Time.deltaTime * 5f);

                // Eğer hasar animasyonu oynamıyorsa hareket et
                if (!animator.GetCurrentAnimatorStateInfo(0).IsName("TakeDamage"))
                {
                    if (mesafe > saldiri_mesafesi)
                    {
                        Vector3 hareket = transform.forward * hareket_hizi;
                        hareket.y = dikey_hiz;
                        controller.Move(hareket * Time.deltaTime);
                        animator.SetBool(runHash, true);
                    }
                    else
                    {
                        animator.SetBool(runHash, false);
                        if (Time.time >= son_saldiri_zamani + saldiri_gecikmesi)
                        {
                            Saldiri();
                        }
                    }
                }
            }
            else
            {
                Vector3 hareket = Vector3.zero;
                hareket.y = dikey_hiz;
                controller.Move(hareket * Time.deltaTime);
                animator.SetBool(runHash, false);
            }
        }

        void Saldiri()
        {
            animator.SetTrigger(attackHash);
            son_saldiri_zamani = Time.time;

            if (player != null)
            {
                PlayerHealth oyuncuCani = player.GetComponent<PlayerHealth>();
                if (oyuncuCani != null)
                {
                    oyuncuCani.TakeDamage((int)saldiri_hasari);
                }
            }
        }

        void OlumAnimasyonunuOynat()
        {
            isDead = true;
            
            // Tüm hareketleri ve davranışları durdur
            animator.SetBool(runHash, false);
            animator.SetTrigger(deathHash);
            
            // Ses efektini çal
            if (olumSesi != null && audioSource != null)
            {
                audioSource.pitch = 1f;
                audioSource.PlayOneShot(olumSesi, sesVolumu);
            }

            // Fizik etkileşimlerini kapat
            if (controller != null)
            {
                controller.enabled = false;
            }

            // Collider'ı trigger yap ki diğer objelerle çarpışmasın
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            // Script'i devre dışı bırak ama GameObject'i yok etme
            enabled = false;

            // 60 saniye sonra yok et
            StartCoroutine(CesediYokEt());
        }

        private IEnumerator CesediYokEt()
        {
            // 60 saniye bekle
            yield return new WaitForSeconds(60f);

            // Yavaşça şeffaflaştırarak yok et
            SkinnedMeshRenderer[] renderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            float fadeTime = 2f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float alpha = 1f - (elapsedTime / fadeTime);

                foreach (SkinnedMeshRenderer renderer in renderers)
                {
                    Material[] materials = renderer.materials;
                    foreach (Material mat in materials)
                    {
                        Color color = mat.color;
                        color.a = alpha;
                        mat.color = color;

                        // Şeffaflık için material ayarlarını güncelle
                        mat.SetFloat("_Surface", 1f); // Transparent
                        mat.SetFloat("_Blend", 0f); // Traditional
                        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                        mat.SetInt("_ZWrite", 0);
                        mat.DisableKeyword("_ALPHATEST_ON");
                        mat.EnableKeyword("_ALPHABLEND_ON");
                        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                        mat.renderQueue = 3000;
                    }
                }
                yield return null;
            }

            // En son objeyi yok et
            Destroy(gameObject);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, algilama_mesafesi);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, saldiri_mesafesi);
        }
    }
} 