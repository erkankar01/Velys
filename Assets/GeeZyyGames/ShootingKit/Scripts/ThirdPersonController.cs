using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

namespace GeeZyyGames
{
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        public float MoveSpeed = 2.0f;
        public float SprintSpeed = 6f;
        public float AimMoveSpeed = 3f;

        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;

        [Space(10)]
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        public float TopClamp = 70.0f;
        public float BottomClamp = -30.0f;
        public float CameraAngleOverride = 0.0f;
        public bool LockCameraPosition = false;

        [Header("Touch Speed")]
        public float touchSpeedPc = 500f;
        public float touchSpeedPcInAim = 150f;

        public float touchSpeedMobile = 4f;
        public float touchSpeedMobileInAim = 1f;

        public bool sprint = true;
        public float smoothTouch = 0.3f;
        public float animationBlendSmooth = 0.1f;
        public float aimDuration = 0.2f;

        private GameObject CinemachineCameraTarget;
        private AudioSource footStepsSource;
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private GameObject _mainCamera;
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private Animator _animator;
        private CharacterController _controller;
        private bool _hasAnimator;
        private Vector3 inputDirection;
        private float targetSpeed;
        private float touchSpeed = 4f;
        private GameManager gameManager;
        private FollowPlayerRoot FollowPlayerRoot;
        private bool jumpBool;
        private WeaponController weaponController;
        private RigBuilder rigBuilder;

        private void Awake()
        {
            ComponentsAdd();
        }

        private void Start()
        {
            _mainCamera = Camera.main.gameObject;
            FollowPlayerRoot = GameObject.FindObjectOfType<FollowPlayerRoot>();
            gameManager = GameObject.FindObjectOfType<GameManager>();

            _controller = GetComponent<CharacterController>();
            weaponController = GetComponent<WeaponController>();
            footStepsSource = GetComponent<AudioSource>();

            FollowPlayerRoot.player = this.transform;
            CinemachineCameraTarget = FollowPlayerRoot.gameObject;
            gameManager.SetupPlayerInputs();
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
            _hasAnimator = TryGetComponent(out _animator);
            AssignAnimationIDs();
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
            InitialPlatformSettings();
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
            PlayerAimMove();
        }

        private void LateUpdate()
        {
            CameraRotation();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        private void GroundedCheck()
        {
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            if (gameManager.mobileInput)
            {
                _cinemachineTargetYaw += gameManager.touch.TouchDist.x * touchSpeed * Time.deltaTime;
                _cinemachineTargetPitch += -gameManager.touch.TouchDist.y * touchSpeed * Time.deltaTime;
            }

            else
            {
                _cinemachineTargetYaw += Input.GetAxis("Mouse X") * touchSpeed * Time.deltaTime;
                _cinemachineTargetPitch += -Input.GetAxis("Mouse Y") * touchSpeed * Time.deltaTime;
            }

            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
            Quaternion angle = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride, _cinemachineTargetYaw, 0.0f);
            CinemachineCameraTarget.transform.rotation = Quaternion.Slerp(CinemachineCameraTarget.transform.rotation, angle, smoothTouch);
        }

        private void PlayerAimMove()
        {
            if (weaponController.aimBool)
            {
                if (gameManager.mobileInput)
                {
                    _animator.SetFloat("moveX", gameManager.joystick.AxisNormalized.normalized.x);
                    _animator.SetFloat("moveY", gameManager.joystick.AxisNormalized.normalized.y);
                }
                else
                {
                    _animator.SetFloat("moveX", gameManager.PcMoveInputs(false).x);
                    _animator.SetFloat("moveY", gameManager.PcMoveInputs(false).z);
                }
            }
        }

        private void Move()
        {
            if (weaponController.aimBool)
            {
                targetSpeed = AimMoveSpeed;
            }

            else if (sprint)
            {
                targetSpeed = SprintSpeed;
            }

            else
            {
                targetSpeed = MoveSpeed;
            }

            if (gameManager.mobileInput)
            {
                if (gameManager.joystick.AxisNormalized.normalized == Vector2.zero)
                {
                    targetSpeed = 0.0f;
                }
            }

            else
            {
                if (inputDirection.magnitude == 0)
                {
                    targetSpeed = 0;
                }
            }

            if (inputDirection.normalized.magnitude > 0.1f)
            {
                _speed = Mathf.Lerp(targetSpeed, targetSpeed * inputDirection.normalized.magnitude,
                    Time.deltaTime * SpeedChangeRate);

                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            if (gameManager.mobileInput)
            {
                inputDirection = new Vector3(gameManager.joystick.AxisNormalized.normalized.x, 0.0f, gameManager.joystick.AxisNormalized.normalized.y);
            }
            else
            {
                inputDirection = new Vector3(gameManager.PcMoveInputs(true).x, 0, gameManager.PcMoveInputs(true).z);
            }

            if (gameManager.mobileInput)
            {
                if (weaponController.aimBool)
                {
                    FullPlayerRotation();
                }

                else if (gameManager.joystick.AxisNormalized.normalized != Vector2.zero)
                {
                    PlayerRotation();
                }
            }

            else
            {
                if (weaponController.aimBool)
                {
                    FullPlayerRotation();
                }

                else if (inputDirection.magnitude != 0)
                {
                    PlayerRotation();
                }
            }


            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputDirection.normalized.magnitude);

            }
            else
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputDirection.normalized.magnitude);
            }



            if (inputDirection.magnitude > 0.5f && Grounded && !gameManager.gamePause)
            {
                if (!footStepsSource.isPlaying)
                {
                    footStepsSource.PlayOneShot(gameManager.footStepsSoundClip, .1f);
                }
            }
        }

        private void PlayerRotation()
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                  _mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        private void FullPlayerRotation()
        {
            _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            transform.rotation = Quaternion.Euler(0.0f, _mainCamera.transform.eulerAngles.y, 0.0f);
        }

        private void JumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (jumpBool && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                jumpBool = false;
            }
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void ComponentsAdd()
        {
            if(gameObject.GetComponent<AudioSource>() == null)
            {
                gameObject.AddComponent<AudioSource>();
            }

            if(gameObject.GetComponent<WeaponController>() == null)
            {
                gameObject.AddComponent<WeaponController>();
            }

            if (rigBuilder == null)
            {
                rigBuilder = gameObject.GetComponent<RigBuilder>();
                if (rigBuilder == null)
                {
                    rigBuilder = gameObject.AddComponent<RigBuilder>();
                }
            }

            RigControl rigControl = GetComponentInChildren<RigControl>();

            if (rigBuilder.layers == null)
            {
                rigBuilder.layers = new System.Collections.Generic.List<RigLayer>();
            }

            if (rigControl.rig1 != null)
            {
                RigLayer newLayer1 = new RigLayer(rigControl.rig1, true);
                rigBuilder.layers.Add(newLayer1);
            }

            if (rigControl.rig2 != null)
            {
                RigLayer newLayer2 = new RigLayer(rigControl.rig2, true);
                rigBuilder.layers.Add(newLayer2);
            }

            if (rigControl.rig3 != null)
            {
                RigLayer newLayer2 = new RigLayer(rigControl.rig3, true);
                rigBuilder.layers.Add(newLayer2);
            }

            rigBuilder.Build();
        }

        public void SetAimTouch()
        {
            if (gameManager.mobileInput)
            {
                touchSpeed = touchSpeedMobileInAim;
                smoothTouch = 0.2f;
            }

            else
            {
                touchSpeed = touchSpeedPcInAim;
                smoothTouch = 0.2f;
            }
        }

        public void SetThirdPersonTouch()
        {
            if (gameManager.mobileInput)
            {
                touchSpeed = touchSpeedMobile;
                smoothTouch = 0.3f;
            }
            else
            {
                touchSpeed = touchSpeedPc;
                smoothTouch = 0.3f;
            }
        }

        private void InitialPlatformSettings()
        {
            if (gameManager.mobileInput)
            {
                touchSpeed = touchSpeedMobile;
            }

            else
            {
                touchSpeed = touchSpeedPc;
            }
        }
    }
}