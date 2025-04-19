using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Animations;
using Cinemachine;
using GeeZyyGames.ShootingKit;

namespace GeeZyyGames
{
    public class WeaponController : MonoBehaviour
    {


        [HideInInspector]
        public bool aimBool;
        public ShootController[] weapons;


        private Animator rigController;
        private ShootController weapon;
        private GameManager gameManager;
        private ShootController shootController;
        private ThirdPersonController thirdPersonController;
        private Animator _animator;
        private RigBuilder rigBuilder;
        private CinemachineVirtualCamera aimCamera;
        private MultiAimConstraint multiAimConstraint; 

        [HideInInspector]
        public bool weaponActiveBool;

        private int currentWeapon = 0;

        private void Start()
        {
            weapons = GetComponentsInChildren<ShootController>();
            rigController = GetComponentInChildren<RigControl>().GetComponent<Animator>();
            aimCamera = GameObject.FindGameObjectWithTag("PlayerAim").GetComponent<CinemachineVirtualCamera>();
            gameManager = FindObjectOfType<GameManager>();

            _animator = GetComponent<Animator>();
            rigBuilder = GetComponent<RigBuilder>();
            multiAimConstraint = GetComponentInChildren<WeaponAim>().gameObject.GetComponent<MultiAimConstraint>();
            thirdPersonController = GetComponent<ThirdPersonController>();

            aimCamera.gameObject.SetActive(false);

            for(int i = 0; i < weapons.Length; i++)
            {
                weapons[i].gameObject.SetActive(false);
            }
        }

        public void PlayerAim()
        {
            if (!weaponActiveBool)
            {
                return;
            }
            shootController = GetComponentInChildren<ShootController>();

            if (aimBool)
            {
                PlayerAimFalse();
            }

            else
            {
                PlayerAimTrue();
            }
        }

        public void PlayerAimTrue()
        {
            if (!weaponActiveBool)
            {
                return;
            }
            shootController = GetComponentInChildren<ShootController>();

            if (shootController.isSniper)
            {
                gameManager.scopeImage.SetActive(true);
                gameManager.aimGraphics.SetActive(false);
            }
            else
            {
                gameManager.aimGraphics.SetActive(true);
            }
            aimCamera.m_Lens.FieldOfView = shootController.weaponZoom;

            Cinemachine3rdPersonFollow transposer = aimCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            transposer.ShoulderOffset = shootController.shoulderOffset;

            aimBool = true;
            rigController.SetBool("AimWeapon", true);
            _animator.SetBool("Aim", true);
            aimCamera.gameObject.SetActive(true);
            for (int i = 0; i < rigBuilder.layers.Count; i++)
            {
                rigBuilder.layers[i].rig.weight = 1;
            }
            multiAimConstraint.weight = 1;
            shootController.gunMesh.SetActive(true);
            gameManager.shootButton.gameObject.SetActive(true);
            gameManager.shootButton1.gameObject.SetActive(true);
            gameManager.reloadButton.gameObject.SetActive(true);
            thirdPersonController.SetAimTouch();
        }

        public void PlayerAimFalse()
        {
            if (!weaponActiveBool)
            {
                return;
            }
            shootController = GetComponentInChildren<ShootController>();

            if (shootController.isSniper)
            {
                gameManager.scopeImage.SetActive(false);
            }

            aimBool = false;
            rigController.SetBool("AimWeapon", false);
            _animator.SetBool("Aim", false);
            aimCamera.gameObject.SetActive(false);

            for (int i = 1; i < rigBuilder.layers.Count; i++)
            {
                rigBuilder.layers[i].rig.weight = 1;
            }
            multiAimConstraint.weight = 0;
            rigBuilder.layers[0].rig.weight = 0;
            gameManager.shootButton.gameObject.SetActive(false);
            gameManager.shootButton1.gameObject.SetActive(false);
            gameManager.reloadButton.gameObject.SetActive(false);
            gameManager.aimGraphics.SetActive(false);
            thirdPersonController.SetThirdPersonTouch();
            shootController.isFiring = false;
        }

        public void ActivePlayerWeapon(int weaponIndex)
        {
            PlayerAimFalse();
            weaponActiveBool = true;
            for(int i = 0; i < weapons.Length; i++)
            {
                weapons[i].gameObject.SetActive(false);
            }
            ShootController newWeapon = weapons[weaponIndex];
            gameManager.SetupShootingInputs(newWeapon);
            newWeapon.gameObject.SetActive(true);
            EquipWeapon(newWeapon);
            for (int i = 1; i < rigBuilder.layers.Count; i++)
            {
                rigBuilder.layers[i].rig.weight = 1;
            }
            multiAimConstraint.weight = 0;
            weapon.gunMesh.SetActive(true);
            gameManager.ammoPanel.SetActive(true);
            gameManager.aimButton.gameObject.SetActive(true);
        }

        public void EquipWeapon(ShootController newWeapon)
        {
            weapon = newWeapon;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;

            rigController.Play("equip_" + weapon.weaponName);
        }

        public void DeactivePlayerWeapon()
        {
            if (aimBool)
            {
                PlayerAimFalse();
            }
            weaponActiveBool = false;

            for (int i = 0; i < rigBuilder.layers.Count; i++)
            {
                rigBuilder.layers[i].rig.weight = 0;
            }
            for(int i = 0; i < weapons.Length; i++)
            {
                weapons[i].gameObject.SetActive(false);
            }
            gameManager.ammoPanel.SetActive(false);
            gameManager.aimButton.gameObject.SetActive(false);
        }

        public void ReloadGun()
        {
            shootController = GetComponentInChildren<ShootController>();
            rigController.Play("reload_" + shootController.weaponName);
            StartCoroutine(shootController.UpdateTextAfterTime(.5f));

            StartCoroutine(weapon.WeaponReload(weapon.weaponReloadTime));
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                SwitchWeapon();
            }
        }

        private void SwitchWeapon()
        {
            if (currentWeapon >= 0 && currentWeapon < weapons.Length)
            {
                weapons[currentWeapon].gameObject.SetActive(false);
            }

            currentWeapon = (currentWeapon + 1) % weapons.Length;

            weapons[currentWeapon].gameObject.SetActive(true);
        }

        public void DealDamageToEnemy(RaycastHit hit)
        {
            CanavarKontrol canavar = hit.collider.GetComponent<CanavarKontrol>();
            if (canavar != null && currentWeapon >= 0 && currentWeapon < weapons.Length)
            {
                string silahTipi = weapons[currentWeapon].weaponType.ToString().ToLower();
                canavar.HasarAl(silahTipi);
            }
        }
    }
}
