using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeeZyyGames
{
    public class EnemyShootController : MonoBehaviour
    {
        public bool patrolEnemy;

        private Transform target;  // The target object to look at
        public float lookSpeed;

        private float nextFire = 0f;
        public float fireRate;

        private Ray ray;
        private RaycastHit hitInfo;

        public Transform raycastOrigin;

        [Header("Effects")]
        public ParticleSystem muzzleEffect;
        public ParticleSystem hitEffect;
        public ParticleSystem[] hitEnemyEffetc;

        public TrailRenderer bulletTrail;


        public LayerMask layers;

        private AudioSource audioSource;

        public AudioClip BulletSound;

        private bool isFiring;
        public float fireStartDelay;
        public float fireFalseRate;
        public float fireTrueRate;

        private Animator animator;

        private Animator rigAnim;

        public int damageRate;

        private float distanceRange = 4f;

        private float currentDistance;

        public float aimAccuracy;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            animator = transform.root.GetComponent<Animator>();
            rigAnim = GetComponentInParent<RigControl>().GetComponent<Animator>();
            StartCoroutine(FiringTrue());

            if (patrolEnemy)
            {
                animator.SetBool("starfe", true);
            }

            target = GameObject.FindObjectOfType<LookForEnemy>().transform;
        }

        void Update()
        {
            RotateTowardsPlayer();
            ShootPlayer();
            CalculateDistance();
        }

        private void RotateTowardsPlayer()
        {
            // Get the position difference between the target and the object
            Vector3 direction = target.position - transform.root.position;

            // Calculate the angle to rotate only on the Y-axis
            float angleY = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // Create a new rotation that only affects the Y-axis
            Quaternion targetRotation = Quaternion.Euler(0f, angleY, 0f);

            // Apply the rotation to the object (you can use Lerp/Slerp for smooth rotation)
            transform.root.rotation = Quaternion.Slerp(transform.root.rotation, targetRotation, lookSpeed);
        }



        private void ShootPlayer()
        {
            if (Time.time > nextFire && isFiring)
            {
                nextFire = Time.time + fireRate;
                FireBullet();
            }
        }

        private void FireBullet()
        {
            rigAnim.Play("recoil_rifle", 1);
            audioSource.PlayOneShot(BulletSound, 0.5f);
            muzzleEffect.Emit(1);

            ray.origin = raycastOrigin.position;


            if (currentDistance < distanceRange)
            {
                ray.direction = (target.position) - raycastOrigin.position;
            }
            else
            {
                ray.direction = (target.position + new Vector3(Random.Range(0f, aimAccuracy), Random.Range(0f, aimAccuracy), Random.Range(0f, aimAccuracy))) - raycastOrigin.position;
            }

            TrailRenderer tracer = Instantiate(bulletTrail, ray.origin, Quaternion.identity);
            tracer.AddPosition(ray.origin);


            if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity, layers, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.gameObject.tag == "Player")
                {
                    for (int i = 0; i < hitEnemyEffetc.Length; i++)
                    {
                        hitEnemyEffetc[i].transform.position = hitInfo.point;
                        hitEnemyEffetc[i].transform.forward = hitInfo.normal;
                        hitEnemyEffetc[i].Emit(1);
                    }

                    tracer.transform.position = hitInfo.point;
                    hitInfo.collider.gameObject.GetComponent<PlayerHealth>().TakeDamage(damageRate);
                }

                else if (hitInfo.collider.gameObject.tag != "Human")
                {
                    hitEffect.transform.position = hitInfo.point;
                    hitEffect.transform.forward = hitInfo.normal;
                    hitEffect.Emit(1);
                }
            }

            else
            {
                tracer.transform.position = target.position;
            }
        }

        private IEnumerator FiringTrue()
        {
            yield return new WaitForSeconds(fireStartDelay);
            while (true)
            {
                isFiring = true;
                animator.SetBool("isFiring", true);

                yield return new WaitForSeconds(fireFalseRate);
                isFiring = false;
                animator.SetBool("isFiring", false);

                yield return new WaitForSeconds(fireTrueRate);
            }
        }

        private void CalculateDistance()
        {
            currentDistance = Vector3.Distance(target.position, transform.position);

            if (currentDistance < distanceRange)
            {
                animator.SetBool("starfe", false);

            }

            else if (patrolEnemy)
            {
                animator.SetBool("starfe", true);
            }
        }
    }
}
