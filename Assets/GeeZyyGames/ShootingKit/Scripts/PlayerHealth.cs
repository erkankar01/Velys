using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

namespace GeeZyyGames
{

    public class PlayerHealth : MonoBehaviour
    {
        public float maxHealth;
        private float currentHealth;

        private Animator animator;
        private AudioSource audioSource;

        public AudioClip damageSound;

        private GameManager gameManager;


        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();

            currentHealth = maxHealth;
            animator = GetComponent<Animator>();
            gameManager.playerHealthBar.fillAmount = currentHealth / maxHealth;
            audioSource = GetComponent<AudioSource>();
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            gameManager.playerHealthBar.fillAmount = (currentHealth / maxHealth);
            audioSource.PlayOneShot(damageSound, .3f);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                animator.Play("Dead");
                GetComponent<ThirdPersonController>().enabled = false;
                GetComponent<CharacterController>().enabled = false;
                GetComponent<RigBuilder>().enabled = false;

                WeaponController weaponController = GetComponent<WeaponController>();

                weaponController.PlayerAimFalse();

                for(int i = 0; i < weaponController.weapons.Length; i++)
                {
                    weaponController.weapons[i].gameObject.transform.GetChild(0).gameObject.SetActive(false);
                }

                weaponController.enabled = false;


                if (gameManager.mobileInput)
                {
                    gameManager.ResetJoystick();
                    gameManager.mobileInputsUi.SetActive(false);
                }

                else
                {
                    gameManager.disablePlayerInputs = true;
                }

                gameManager.retryPanel.SetActive(true);
                gameManager.CursorUnlocked();
            }
        }
    }
}
