using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;
using UnityEngine.SceneManagement;

namespace GeeZyyGames
{
    public class GameManager : MonoBehaviour
    {
        public bool mobileInput;
        [Header("Player Mobile Inputs")]
        public Joystick joystick;
        public TouchField touch;
        public Button aimButton;
        public Button shootButton;
        public Button shootButton1;
        public Button reloadButton;
        public Button WeaponButton;

        [Header("Managers")]
        private WeaponController weaponController;
        private ShootController shootController;

        [Header("UI Stats")]
        public Text ammoText;
        public Text maxBulletText;
        public GameObject gameUi;
        public GameObject aimGraphics;
        public GameObject mobileInputsUi;
        public GameObject weaponPanel;
        public GameObject ammoPanel;
        public GameObject controlsPanel;
        public GameObject controlsText;
        public GameObject scopeImage;
        public Image playerHealthBar;
        public GameObject mobileUi;
        public GameObject retryPanel;

        [Header("Sounds Clips")]
        public AudioClip footStepsSoundClip;
        public AudioClip collectSoundClip;

        [Header("Audio Sources")]
        public AudioSource effectsSource;

        [Header("PC Inputs")]

        [HideInInspector]
        public float horizontalMove;
        [HideInInspector]
        public float verticalMove;
        [HideInInspector]
        public bool gamePause;

        [HideInInspector]
        public bool disablePlayerInputs;

        private void Start()
        {
            SetupPlatform();
            SetupWeaponButton();
            SetupAimButton();
        }

        private void Update()
        {
            PcMouseInputs();
        }

        public Vector3 PcMoveInputs(bool rawInput)
        {
            if (rawInput)
            {
                horizontalMove = Input.GetAxisRaw("Horizontal");
                verticalMove = Input.GetAxisRaw("Vertical");
            }
            else
            {
                horizontalMove = Input.GetAxis("Horizontal");
                verticalMove = Input.GetAxis("Vertical");
            }

            return new Vector3(horizontalMove, 0, verticalMove);
        }

        public void PcMouseInputs()
        {
            if (mobileInput || gamePause || disablePlayerInputs)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                shootController.ResetBullets();
            }

            if (Input.GetMouseButtonDown(0))
            {
                CursorLocked();
            }

            if (Input.GetMouseButtonDown(1))
            {
                weaponController.PlayerAimTrue();
            }

            if (Input.GetMouseButtonUp(1))
            {
                weaponController.PlayerAimFalse();
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                ControlsOpen();
            }

            if (weaponController.aimBool)
            {
                if (Input.GetMouseButton(0))
                {
                    shootController.FirePressDown();
                }
                else
                {
                    shootController.FirePressUp();
                }

                if (Input.GetKeyDown(KeyCode.R))
                {
                    weaponController.ReloadGun();
                }
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CursorUnlocked();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                ActiveWeaponPanel();
            }
        }

        public void SetupPlayerInputs()
        {
            weaponController = GameObject.FindObjectOfType<WeaponController>();
            SetupReloadButton();
        }

        public void SetupShootingInputs(ShootController shoot)
        {
            shootController = shoot;
            shootButton.gameObject.AddComponent<EventTrigger>();
            shootButton1.gameObject.AddComponent<EventTrigger>();

            SetupPlayerShootDown();
            SetupPlayerShootUp();
        }

        private void SetupWeaponButton()
        {
            WeaponButton.onClick.AddListener(() => ActiveWeaponPanel());
        }

        private void SetupReloadButton()
        {
            reloadButton.onClick.AddListener(() => weaponController.ReloadGun());
        }

        private void SetupAimButton()
        {
            aimButton.onClick.AddListener(() => weaponController.PlayerAim());
        }

        private void SetupPlayerShootDown()
        {
            EventTrigger.Entry pointer = new EventTrigger.Entry();
            pointer.eventID = EventTriggerType.PointerDown;
            pointer.callback.AddListener((e) => shootController.FirePressDown());

            shootButton.GetComponent<EventTrigger>().triggers.Add(pointer);
            shootButton1.GetComponent<EventTrigger>().triggers.Add(pointer);
        }

        private void SetupPlayerShootUp()
        {
            EventTrigger.Entry pointer = new EventTrigger.Entry();
            pointer.eventID = EventTriggerType.PointerUp;
            pointer.callback.AddListener((e) => shootController.FirePressUp());

            shootButton.GetComponent<EventTrigger>().triggers.Add(pointer);
            shootButton1.GetComponent<EventTrigger>().triggers.Add(pointer);
        }

        public void UpdateAmmoText(int ammo)
        {
            ammoText.text = ammo.ToString();
        }

        public void PlayClip(AudioClip clip, float vol)
        {
            effectsSource.PlayOneShot(clip, vol);
        }

        private void SetupPlatform()
        {
            if (mobileInput)
            {
                mobileInputsUi.SetActive(true);
                controlsText.SetActive(false);
            }
            else
            {
                mobileInputsUi.SetActive(false);
                controlsText.SetActive(true);
                CursorLocked();
            }
        }

        private void GameUnpause()
        {
            gamePause = false;
            Time.timeScale = 1f;
            weaponPanel.SetActive(false);
            CursorLocked();
        }

        public void ActiveWeapon(int weaponIndex)
        {
            GameUnpause();
            weaponController.ActivePlayerWeapon(weaponIndex);
        }

        public void DeactiveWeapon()
        {
            GameUnpause();
            weaponController.DeactivePlayerWeapon();
        }

        private void ActiveWeaponPanel()
        {
            Time.timeScale = 0f;
            weaponPanel.SetActive(true);
            CursorUnlocked();
            gamePause = true;
        }

        private void CursorLocked()
        {
            if (!mobileInput)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void CursorUnlocked()
        {
            if (!mobileInput)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void ControlsOpen()
        {
            controlsPanel.SetActive(true);
            Time.timeScale = 0;
            CursorUnlocked();
            gamePause = true;
        }

        public void ControlsClose()
        {
            Time.timeScale = 1;
            controlsPanel.SetActive(false);
            CursorLocked();
            gamePause = false;
        }

        public void ResetJoystick()
        {
            joystick.Pressed = false;
            joystick.InputVector = Vector2.zero;
            joystick.Handle.anchoredPosition = Vector2.zero;

            touch.Pressed = false;
            touch.TouchDist = Vector2.zero;
        }

        public void ReloadScene(int index)
        {
            SceneManager.LoadScene(index);
        }
    }
}
