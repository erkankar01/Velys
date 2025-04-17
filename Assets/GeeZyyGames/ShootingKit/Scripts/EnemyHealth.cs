using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace GeeZyyGames
{
    public class EnemyHealth : MonoBehaviour
    {
        public int maxHealth;
        private int currentHealth;

        private Animator animator;

        private GameObject gunMesh;


        public AudioClip deathSound;

        private bool deadBool;

        private void Start()
        {
            currentHealth = maxHealth;
            animator = GetComponent<Animator>();
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;

            if (currentHealth <= 0 && !deadBool)
            {
                deadBool = true;
                animator.SetBool("dead", true);
                GetComponent<RigBuilder>().enabled = false;
                GetComponentInChildren<EnemyShootController>().enabled = false;
                gunMesh = GetComponentInChildren<EnemyShootController>().transform.gameObject;
                gunMesh.SetActive(false);
                GetComponent<AudioSource>().PlayOneShot(deathSound, 1f);
            }
        }
    }
}
