using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.InputSystem;
using System;
using Sfs2X.Entities;
using Cinemachine;

/**
* The SFS Character controller is added to both Player and remote characters on Instantiate.
* The bool isPlayer is used to separate Player methods from remote character methods.
*/
public class SF2X_CharacterController : MonoBehaviour
{
    private static SF2X_CharacterController instance;
    public static SF2X_CharacterController Instance
    {
        get
        {
            return instance;
        }
    }
    private bool IsCurrentDeviceMouse
    {
        get
        {
            return true;
        }
    }

    /**
    * Required varaiables for the SFS Character Controller
    * Further information can be found in the documentation
    */

    #region  Global Definitions
    public bool help;
    public bool music;
    private const float _threshold = 0.01f;
    private AudioSource audioSource;
    private SF2X_SyncManager syncManager;
    private bool simpleLerp;
    public bool isPlayer;
    public int userid;
    private Animator animator;
    public Transform spineTransform;
    public static readonly float sendingPeriod = 0.03f;
    private float timeLastSending = 0.0f;
    public bool send = false;
    public SF2X_CharacterTransform lastState;
    private SF2X_CharacterTransform newState;
    #endregion

    #region  Character Movement Definitions
    public float MoveSpeed = 4.0f;
    public float SprintSpeed = 6.0f;
    public float RotationSpeed = 1.3f;
    public float SpeedChangeRate = 10.0f;
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;
    public float JumpTimeout = 0.1f;
    public float FallTimeout = 0.15f;
    public static int InputHorizontal;
    public static int InputVertical;
    public static int InputMagnitude;
    public static int IsGrounded;
    public static int IsStrafing;
    public static int IsSprinting;
    public static int GroundDistance;
    public float rotSpeed = 250;
    public float damping = 10;
    private Vector2 lastDirection;
    private Vector2 relativeDirection;
    private bool speedChange;
    Vector3 inputDirection;
    Vector3 relativeInput;
    float verticalSpeed;
    float horizontalSpeed;
    private float SetMoveSpeed = 3.0f;
    private float targetSpeed;
    public Vector2 move;
    public Vector2 look;
    public const float walkSpeed = 0.5f;
    public const float runningSpeed = 1.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;
    private Vector3 colExtents;
    private CharacterController _controller;
    public bool isMoving;
    public bool isAiming;
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.5f;
    public LayerMask GroundLayers;
    #endregion

    #region  Animation Definitions
    public bool dead;
    public bool died;
    public bool jump;
    public bool jumping;
    public bool sprint;
    public bool stopMove;
    public bool reloading;
    public bool aim;
    private int walkforward;
    private int walkbackward;
    private int walkleft;
    private int walkright;
    private int runforward;
    private int runbackward;
    private int runleft;
    private int runright;
    private int shoot;
    private int reload;
    private int aiming;
    private int idle;
    private int respawn;
    private int wounded;
    private int die;
    private int inair;
    private int grounded;
    private int Jump;
    #endregion

    #region  Cinemachine Definitions
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 20.0f;
    public float BottomClamp = -20.0f;
    public int view = 2;
    public int crosshair = 0;
    private Camera cam;
    public float CameraAngleOverride = 0.0f;
    public bool LockCameraPosition = true;
    private float _cinemachineTargetPitch;
    [Range(0.0f, 0.3f)]
    public float RotationSmoothTime = 0.2f;
    #endregion

    /**
    * Update and LateUpdate methods are kept separate 
    * to ensure the correct execution order
    */

    #region  Unity Methods

    private void Awake()
    {
        instance = this;
        colExtents = GetComponent<Collider>().bounds.extents;
    }

    private void Start()
    {
        cam = Camera.main;
        _controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        syncManager = GetComponent<SF2X_SyncManager>();
        AssignAnimationIDs();
        jumpTimeoutDelta = JumpTimeout;
        fallTimeoutDelta = FallTimeout;
        if (isPlayer)
        {
            this.transform.GetComponentInChildren<SF2X_AimIK>().isPlayer = true;
            spineTransform = GetComponentInChildren<SF2X_AimIK>().spine;
        }
        else
        {
            spineTransform = animator.GetBoneTransform(HumanBodyBones.Spine).transform;
        }
        isMoving = false;
        audioSource = this.transform.GetComponent<AudioSource>();
    }

 

    private void Update()
    {
        GroundedCheck();
        if (isPlayer)
        {
            Move();
            Animate();
            JumpAndGravity();
            SendTransform();
        }
    }

    private void LateUpdate()
    {
        if (isPlayer)
        {
            CameraRotation();
            MoveTransform();
        }
        if (!isPlayer)
        {
            checkPosition();
        }
    }
    #endregion

    /**
    * Camera rotation is controlled by the mouse rotation
    */

    #region  Camera Methods
    private void CameraRotation()
    {
 
       if ((UnityEngine.Cursor.lockState == CursorLockMode.Locked))
        {
            if (look.sqrMagnitude >= _threshold)
            {
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.smoothDeltaTime;
                rotationVelocity = look.x * RotationSpeed * deltaTimeMultiplier;
                if (SF2X_GameManager.invertMouseY)
                     _cinemachineTargetPitch += look.y * RotationSpeed * deltaTimeMultiplier;
                else
                     _cinemachineTargetPitch += -look.y * RotationSpeed * deltaTimeMultiplier;

                _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
                CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            if (!dead)
                {
                   this.transform.Rotate(Vector3.up * rotationVelocity);
                }
            }
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
    #endregion

    /**
    * Character control utilises Unity's new Input System
    * Ground Check is done with a Raycast
    * Animations are sent to the Server Script along with the Transform Position
    */

    #region  Character Movement Control
    private void GroundedCheck()
    {
        Ray ray = new Ray(this.transform.position + Vector3.up * 2 * colExtents.x, Vector3.down);
        if (animator)
        {
            animator.SetBool(grounded, Physics.SphereCast(ray, colExtents.x, colExtents.x + 0.3f));
        }
    }

    private void Animate()
    {
        if ((lastDirection != relativeDirection) || (speedChange))
        {
            switch (relativeDirection.ToString())
            {
                case "(1.00, 0.00)":
                    {
                        if (!sprint)
                        {
                            AnimationReset();
                            this.animator.SetBool(walkforward, true);
                            SF2X_GameManager.Instance.SendAnimationState("walkforward");
                        }
                        else
                        {
                            AnimationReset();
                            this.animator.SetBool(runforward, true);
                            SF2X_GameManager.Instance.SendAnimationState("runforward");
                        }
                    }
                    break;
                case "(-1.00, 0.00)":
                    {
                        if (!sprint)
                        {
                            AnimationReset();
                            this.animator.SetBool(walkbackward, true);
                            SF2X_GameManager.Instance.SendAnimationState("walkbackward");
                        }
                        else
                        {
                            AnimationReset();
                            this.animator.SetBool(runbackward, true);
                            SF2X_GameManager.Instance.SendAnimationState("runbackward");
                        }
                    }
                    break;
                case "(0.00, -1.00)":
                    {
                        if (!sprint)
                        {
                            AnimationReset();
                            this.animator.SetBool(walkleft, true);
                            SF2X_GameManager.Instance.SendAnimationState("walkleft");
                        }
                        else
                        {
                            AnimationReset();
                            this.animator.SetBool(runleft, true);
                            SF2X_GameManager.Instance.SendAnimationState("runleft");
                        }
                    }
                    break;
                case "(0.00, 1.00)":
                    {
                        if (!sprint)
                        {
                            AnimationReset();
                            this.animator.SetBool(walkright, true);
                            SF2X_GameManager.Instance.SendAnimationState("walkright");
                        }
                        else
                        {
                            AnimationReset();
                            this.animator.SetBool(runright, true);
                            SF2X_GameManager.Instance.SendAnimationState("runright");
                        }
                    }
                    break;
                case "(0.00, 0.00)":
                    {
                        AnimationReset();
                        this.animator.SetBool(idle, true);
                        SF2X_GameManager.Instance.SendAnimationState("idle");
                    }
                    break;
            }
            lastDirection = relativeDirection;
            speedChange = false;
        }
    }

    private void Move()
    {
        if (sprint)
        {
            targetSpeed = runningSpeed * SetMoveSpeed;
        }
        else
        {
            targetSpeed = walkSpeed * SetMoveSpeed;
        }
        if (move == Vector2.zero) targetSpeed = 0.0f;
        inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;
        if (move != Vector2.zero)
        {
            inputDirection = transform.right * move.x + transform.forward * move.y;
        }
        relativeInput = transform.InverseTransformDirection(inputDirection);
        verticalSpeed = relativeInput.z;
        horizontalSpeed = relativeInput.x;
        relativeDirection = new Vector2(verticalSpeed, horizontalSpeed);
    }
    private void MoveTransform()
    {
        if (_controller.enabled == true &&  !dead)
            _controller.Move(inputDirection.normalized * Time.deltaTime * targetSpeed + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void JumpAndGravity()
    {
        if (animator.GetBool("Grounded")) 
        {
            fallTimeoutDelta = FallTimeout;
            if (animator)
            {
                animator.SetBool(Jump, false);
             }
            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -10f;
                jumping = false;
            }
            if (jump && jumpTimeoutDelta <= 0.0f)
            {
                verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                if (animator)
                {
                    animator.SetBool(Jump, true);
                }
            }
            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            jumpTimeoutDelta = JumpTimeout;

            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
        }
        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Gravity * Time.deltaTime;
        }
        jump = false;
    }

    #endregion

    /**
    * Unity's new Input System provides an Input value
    * and the following methods are called from each define in the Input Asset
    * To know more about using Unity's New Input System, check out the Unity Documentation
    */

    #region  On Input Methods
    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (jumping == false)
        {
            jump = value.isPressed;
            jumping = true;
        }
    }

    public void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
            sprint = sprint ? false : true;
            speedChange = true;
        }
    }

    public void OnShoot(InputValue value)
    {
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        if (!dead && !died)
        {
            int target;
            target = 99999;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                string tag = hit.collider.tag;
                if (tag == "Player")
                {
                    target = hit.collider.gameObject.GetComponent<SF2X_CharacterController>().userid;
                }
        
              if (!reloading && aim)
                SF2X_GameManager.Instance.SendShot(target);
     
            }
           
        }
        else if (died)
        {
            died = false;
            SF2X_GameManager.Instance.ResurrectPlayer();
   
        }
    }

    public void OnReload(InputValue value)
    {
        SF2X_GameManager.Instance.SendReload();
    }
    public void OnHelp(InputValue value)
    {
        if (value.isPressed)
        {
            help = help ? false : true;
            if (help)
                SF2X_GameManager.Instance.fadeHelpIn(SF2X_GameManager.Instance.helpCanvas);
            else SF2X_GameManager.Instance.fadeHelpOut(SF2X_GameManager.Instance.helpCanvas);
        }
    }

    public void OnAim(InputValue value)
    {
        if (value.isPressed)
        {
            aim = aim ? false : true;
            this.animator.SetBool(aiming, aim);
            if (aim)
                SF2X_GameManager.Instance.SendAnimationState("aiming");
            else
                SF2X_GameManager.Instance.SendAnimationState("notaiming");
        }
    }

    public void OnView(InputValue value)
    {
        togglevView();
    }
    public void OnMusic(InputValue value)
    {
        music = music ? false : true;
        if (music)
            SF2X_GameManager.Instance.stopMusic();
        else SF2X_GameManager.Instance.playMusic();
    }

    public void OnCrosshair(InputValue value)
    {
        toggleCrossHair();
    }
    public void OnPlus(InputValue value)
    {
        RotationSpeed = RotationSpeed + 0.3f;
        if (RotationSpeed > 3.3f)
            RotationSpeed = 3.3f;
    }
    public void OnMinus(InputValue value)
    {
        RotationSpeed = RotationSpeed - 0.3f;
        if (RotationSpeed < 0.3f)
            RotationSpeed = 0.3f;
    }
    public void OnExit(InputValue value)
    {
        SF2X_GameManager.Instance.OnExitGame();
    }
    private void toggleCrossHair()
    {
        for (int i = 0; i < SF2X_GameManager.Instance.crossHairs.Length; i++)
        {
            SF2X_GameManager.Instance.crossHairs[i].SetActive(false);
        }
        crosshair++;
        if (crosshair == SF2X_GameManager.Instance.crossHairs.Length) crosshair = 0;
        SF2X_GameManager.Instance.crossHairs[crosshair].SetActive(true);
    }


    private void togglevView()
    {
        view++;
        switch (view)
        {
            case 1:
                {
                    SF2X_GameManager.Instance.cinemachine3rdPersonFollow.ShoulderOffset = SF2X_GameManager.Instance.firstShoulderOffset;
                }
                break;
            case 2:
                {
                    SF2X_GameManager.Instance.cinemachine3rdPersonFollow.ShoulderOffset = SF2X_GameManager.Instance.secondShoulderOffset;
                }
                break;
            case 3:
                {
                    SF2X_GameManager.Instance.cinemachine3rdPersonFollow.ShoulderOffset = SF2X_GameManager.Instance.thirdShoulderOffset;
                }
                break;
        }
        if (view == 3) view = 0;
    }

    public void OnLook(InputValue value)
    {
        look = value.Get<Vector2>();
    }

    public void OnEscape(InputValue value)
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;

    }
    #endregion

    /**
    * Unity's Animation State Machine controls the Character Aniamtions
    * for both Player and Remote Characters. 
    * Blend trees have NOT been utilised as to enable the animations to be synchronised
    * with the Transform sent from the Server Code.
    */
    
    #region  Character Animation Methods
    private void AssignAnimationIDs()
    {
        walkforward = Animator.StringToHash("WalkForward");
        walkbackward = Animator.StringToHash("WalkBackward");
        walkleft = Animator.StringToHash("WalkLeft");
        walkright = Animator.StringToHash("WalkRight");
        runforward = Animator.StringToHash("RunForward");
        runbackward = Animator.StringToHash("RunBackward");
        runleft = Animator.StringToHash("RunLeft");
        runright = Animator.StringToHash("RunRight");
        aiming = Animator.StringToHash("Aiming");
        shoot = Animator.StringToHash("Shoot");
        reload = Animator.StringToHash("Reload");
        wounded = Animator.StringToHash("Wounded");
        die = Animator.StringToHash("Die");
        respawn = Animator.StringToHash("Respawn");
        idle = Animator.StringToHash("idle");
        grounded = Animator.StringToHash("Grounded");
        Jump = Animator.StringToHash("Jump");
        inair = Animator.StringToHash("InAir");
        AnimationSync("idle");
    }
    public void AnimationReset()
    {
        if (animator)
        {
            this.animator.SetBool(walkforward, false);
            this.animator.SetBool(walkbackward, false);
            this.animator.SetBool(walkleft, false);
            this.animator.SetBool(walkright, false);
            this.animator.SetBool(runforward, false);
            this.animator.SetBool(runbackward, false);
            this.animator.SetBool(runleft, false);
            this.animator.SetBool(runright, false);
            this.animator.SetBool(idle, false);
         }
    }

    public async void AnimationSync(string value)
    {
        if (animator)
        {
            if (value == "shoot")
            {
                this.animator.SetBool(aiming, true);
                this.animator.SetBool(shoot, true);
                await Task.Delay(500);
                this.animator.SetBool(shoot, false);
            }
            if (value == "reload")
            {
                reloading = true;
                this.animator.SetBool(reload, true);
                await Task.Delay(500);
                this.animator.SetBool(reload, false);
                await Task.Delay(2000);
                reloading = false;
            }
            if (value == "wounded")
            {
                audioSource.PlayOneShot(SF2X_GameManager.Instance.wounded);
                this.animator.SetBool(wounded, true);
                await Task.Delay(500);
                this.animator.SetBool(wounded, false);
            }
            if (value == "aiming")
            {
                this.animator.SetBool(aiming, true);
            }
            if (value == "notaiming")
            {
                this.animator.SetBool(aiming, false);
            }
            if (value == "die")
            {
                audioSource.PlayOneShot(SF2X_GameManager.Instance.wounded);
                this.animator.SetBool(wounded, true);
                await Task.Delay(500);
                this.animator.SetBool(wounded, false);

                this.animator.SetBool(aiming, false);
                this.animator.SetBool(respawn, false);

                this.animator.SetBool(die, true);
            }
            if (value == "respawn")
            {
                this.animator.SetBool(aiming, false);
                this.animator.SetBool(die, false);
                this.animator.SetBool(wounded, false);
                this.animator.SetBool(respawn, true);
            }
            while (isMoving == false)
            {
                await Task.Yield();
            }

            switch (value)
            {
                case "idle":
                    {
                        AnimationReset();
                        this.animator.SetBool(idle, true);
                    }
                    break;
                case "walkforward":
                    {
                        AnimationReset();
                        this.animator.SetBool(walkforward, true);
                    }
                    break;
                case "walkbackward":
                    {
                        AnimationReset();
                        this.animator.SetBool(walkbackward, true);
                    }
                    break;
                case "walkleft":
                    {
                        AnimationReset();
                        this.animator.SetBool(walkleft, true);
                    }
                    break;
                case "walkright":
                    {
                        AnimationReset();
                        this.animator.SetBool(walkright, true);
                    }
                    break;
                case "runforward":
                    {
                        AnimationReset();
                        this.animator.SetBool(runforward, true);
                    }
                    break;
                case "runbackward":
                    {
                        AnimationReset();
                        this.animator.SetBool(runbackward, true);
                    }
                    break;
                case "runleft":
                    {
                        AnimationReset();
                        this.animator.SetBool(runleft, true);
                    }
                    break;
                case "runright":
                    {
                        AnimationReset();
                        this.animator.SetBool(runright, true);
                    }
                    break;
            }
        }
    }

    private void Event_RightStep(AnimationEvent animationEvent)
    {
        audioSource.PlayOneShot(SF2X_GameManager.Instance.footstep);
    }
    private void Event_LeftStep(AnimationEvent animationEvent)
    {
        audioSource.PlayOneShot(SF2X_GameManager.Instance.footstep);
    }

    private void Event_OnLand(AnimationEvent animationEvent)
    {
        audioSource.PlayOneShot(SF2X_GameManager.Instance.landing);
    }
    #endregion

    /**
    * If this Script is on the Local Players (isPlayer) SendTransform is called.
    * If this is a remote character, checkPosition is called every frame.
    * The ReceiveTransform method is called to update the position
    * and depends which Network Sync mode is used.
    */

    #region  Transform Synchronisation
    public void SendTransform()
    {
        if (isPlayer)
        {
            if (timeLastSending >= sendingPeriod)
            {
                lastState = SF2X_CharacterTransform.FromTransform(this.transform, this.spineTransform);
                SF2X_GameManager.Instance.SendTransform(lastState);
                timeLastSending = 0;
                return;
            }
            timeLastSending += Time.deltaTime;
        }
    }

    public void ReceiveTransform(SF2X_CharacterTransform chtransform)
    {
        if (!isPlayer)
        {
            switch (SF2X_GameManager.Instance.NetworkSyncMode)
            {
                case SF2X_GameManager.InterpolationMode.Simple:  //SimpleLerp
                    {
                        newState = chtransform;
                        simpleLerp = true;
                    }
                    break;
                case SF2X_GameManager.InterpolationMode.Complex:  //Interpolation
                    {
                        if (syncManager)
                            syncManager.ReceivedTransform(chtransform);
                    }
                    break;
            }
        }
    }

    public void checkPosition()
    {
        if (!isPlayer)
        {
            if (simpleLerp)
            {
                isMoving = true;
                if (this.transform.position != newState.Position)
                {
                    this.transform.position = Vector3.Lerp(this.transform.position, newState.Position, Time.deltaTime * 100.0f);
                }
                if (spineTransform.localRotation != Quaternion.Euler(newState.SpineRotationFPS))
                {
                    spineTransform.localRotation = Quaternion.Euler(newState.SpineRotationFPS);
                }
                if (this.transform.localEulerAngles != newState.AngleRotationFPS)
                {
                    this.transform.localEulerAngles = newState.AngleRotationFPS;
                }
            }
            else
            {
                spineTransform.localRotation = syncManager.spineRotation;
            }
        }
    }
    #endregion
}
