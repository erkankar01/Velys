using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeeZyyGames
{
    public enum SilahTipi
    {
        Pistol,
        Rifle,
        Sniper,
        Shotgun
    }

    public class ShootController : MonoBehaviour
    {
        public bool isSniper;
        public SilahTipi weaponType = SilahTipi.Rifle; // Varsayılan olarak tüfek

        [HideInInspector]
        public bool isFiring;
        public ParticleSystem muzzleEffect;
        public ParticleSystem hitEffect;
        public ParticleSystem[] hitEnemyEffetc;
        public Transform raycastOrigin;
        public TrailRenderer bulletTrail;
        public float fireRate;
        public string weaponName;
        private int currentAmmo;
        public int clipSize;
        public int maxMag;
        private int maxBullets;
        public float weaponReloadTime;
        public GameObject gunMesh;
        public AudioClip weaponSound;
        public AudioClip reloadSound;
        public int damageAmount = 5;
        public LayerMask layerMask;
        public int weaponZoom;
        public Vector3 shoulderOffset;

        private Ray ray;
        private RaycastHit hitInfo;
        private float nextFire = 0f;
        private Transform raycastDestination;
        private GameManager gameManager;
        private Animator rigController;
        private bool reloadOnce;
        private AudioSource ad;

        [HideInInspector]
        public Cinemachine.CinemachineImpulseSource cameraShake;


        private void Awake()
        {
            raycastDestination = GameObject.FindGameObjectWithTag("CrossHair").transform;
            gameManager = GameObject.FindObjectOfType<GameManager>();
            cameraShake = GetComponent<Cinemachine.CinemachineImpulseSource>();
            ad = GetComponent<AudioSource>();
            rigController = GetComponentInParent<Animator>();
            ad.volume = 0.5f;
            currentAmmo = clipSize;
            maxBullets = maxMag;
        }

        private void OnEnable()
        {
            UpdateBullets();
            StartCoroutine(WeaponReload(0));
        }

        private void Update()
        {
            if (isFiring && Time.time > nextFire)
            {
                nextFire = Time.time + fireRate;
                FireBullet();
            }

            if (currentAmmo <= 0 && !reloadOnce)
            {
                StartCoroutine(WeaponReload(weaponReloadTime));
                reloadOnce = true;

            }
        }

        public void FirePressDown()
        {
            isFiring = true;

        }

        public void FirePressUp()
        {
            isFiring = false;
        }

        public IEnumerator WeaponReload(float time)
        {
            yield return new WaitForSeconds(time);

            if (maxBullets > 0 && currentAmmo < clipSize)
            {
                int bulletsNeeded = clipSize - currentAmmo;
                int bulletsToReload = Mathf.Min(bulletsNeeded, maxBullets);

                currentAmmo += bulletsToReload;
                maxBullets -= bulletsToReload;
            }

            else
            {
                yield return null;
            }

            rigController.Play("reload_" + weaponName);
            StartCoroutine(UpdateTextAfterTime(.5f));
        }

        public IEnumerator UpdateTextAfterTime(float time)
        {
            yield return new WaitForSeconds(time);
            gameManager.UpdateAmmoText(currentAmmo);
            gameManager.maxBulletText.text = maxBullets.ToString();
            ad.clip = reloadSound;
            ad.Play();
        }

        private void FireBullet()
        {
            if (rigController.GetCurrentAnimatorStateInfo(0).IsName("reload_" + weaponName))
            {
                return;
            }

            if (currentAmmo <= 0)
            {
                return;
            }

            reloadOnce = false;

            currentAmmo--;

            gameManager.UpdateAmmoText(currentAmmo);

            muzzleEffect.Emit(1);
            ray.origin = raycastOrigin.position;
            ray.direction = raycastDestination.position - raycastOrigin.position;

            TrailRenderer tracer = Instantiate(bulletTrail, ray.origin, Quaternion.identity);
            tracer.AddPosition(ray.origin);

            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layerMask, QueryTriggerInteraction.Ignore))
            {
                WeaponController weaponController = GetComponentInParent<WeaponController>();
                if (weaponController != null)
                {
                    weaponController.DealDamageToEnemy(hitInfo);
                }

                if (hitInfo.collider.CompareTag("Enemy"))
                {
                    for (int i = 0; i < hitEnemyEffetc.Length; i++)
                    {
                        hitEnemyEffetc[i].transform.position = hitInfo.point;
                        hitEnemyEffetc[i].transform.forward = hitInfo.normal;
                        hitEnemyEffetc[i].Emit(1);
                    }
                }
                else
                {
                    hitEffect.transform.position = hitInfo.point;
                    hitEffect.transform.forward = hitInfo.normal;
                    hitEffect.Emit(1);
                }

                tracer.transform.position = hitInfo.point;
            }
            else
            {
                tracer.transform.position = raycastDestination.position;
            }

            cameraShake.GenerateImpulse(Camera.main.transform.forward);
            rigController.Play("recoil_" + weaponName, 1, 0.0f);
            ad.clip = weaponSound;
            ad.Play();
        }

        private void UpdateBullets()
        {
            gameManager.UpdateAmmoText(currentAmmo);
            gameManager.maxBulletText.text = maxBullets.ToString();
        }

        public void ResetBullets()
        {
            currentAmmo = clipSize;
            maxBullets = maxMag;
            UpdateBullets();
        }
    }
}