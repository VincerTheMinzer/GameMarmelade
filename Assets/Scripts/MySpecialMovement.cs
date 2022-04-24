using System;
using UnityEngine;

public class MySpecialMovement : MonoBehaviour
{
    private Rigidbody _rb;
    public GameObject MyCameraGO;
    public Transform MyCameraTrans;
    public float PlayerHeight = 2;
    public GameObject Respawn;

    //[SerializeField] private Animator _anim;

    //MOVEMENT
    [Header("Movement Variables")]

    [SerializeField] float _MovingTurnSpeed = 360;
    [SerializeField] float _StationaryTurnSpeed = 180;

    float _CapsuleHeight;
    Vector3 _CapsuleCenter;
    CapsuleCollider _Capsule;

    Vector3 _GroundNormal;

    public Transform Knee;
    public Transform Chest;


    // ----------
    private FrameInputs _inputs;

    // Start is called before the first frame update
    void Start()
    {
        _Animator = GetComponent<Animator>();

        _Capsule = GetComponent<CapsuleCollider>();
        _CapsuleHeight = _Capsule.height;
        _CapsuleCenter = _Capsule.center;

        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    // Update is called once per frame
    void Update()
    {

        var move = _rb.velocity;
        //Debug.Log(_inputs.X + ", " + _rb.velocity + ", " + IsGrounded);
        if (move.magnitude > 1f) move.Normalize();
        move = transform.InverseTransformDirection(move);

        HandleGrounding();

        move = Vector3.ProjectOnPlane(move, _GroundNormal);
        _TurnAmount = Mathf.Atan2(move.x, move.z);
        _ForwardAmount = move.z;

        ApplyExtraTurnRotation();

        HandleWalking();

        HandleJumping();

        ScaleCapsuleForCrouching(Input.GetKey(KeyCode.S));
        PreventStandingInLowHeadroom();

        UpdateAnimator(move);
    }

    private bool isClimbing = false;
    private void OnTriggerEnter(Collider other)
    {
        DoDaClimb();
    }

    public void DoDaClimb()
    {
        Debug.Log("CLIMB BITCH CLIMB");
        isClimbing = true;
        Vector3 climb = (transform.up*8 + transform.right)*2;
        _rb.velocity = Vector3.MoveTowards(_rb.velocity, climb, 50);
        //Vector3 daPos = transform.position;
        //daPos.y += PlayerHeight;
        //transform.position = daPos;
    }


    public void IncreaseSpeedBy(float spd)
    {
        _walkSpeed *= spd * 0.5f;
        _acceleration *= spd * 0.5f;
    }

    public void die()
    {
        GetComponent<Health>().DamagePlayer(100);
        GameObject myres = Instantiate(Respawn, Camera.main.transform);
        transform.position = myres.transform.GetChild(0).position;
        myres.transform.parent = null;
    }

    

    #region Movement

    #region Detection

    [Header("Detection")]
    [SerializeField] private float _groundCheckDistance = 0.3f;
    public bool IsGrounded;
    public static event Action OnTouchedGround;
    public Transform Foot;

    private readonly Collider[] _ground = new Collider[1];

    private void HandleGrounding()
    {
        RaycastHit hitInfo;
        // Grounder
        //Debug.DrawLine(Foot.position + (Vector3.up * 0.1f), Foot.position + (Vector3.up * 0.1f) + (Vector3.down * _groundCheckDistance));
        bool grounded = Physics.Raycast(Foot.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, _groundCheckDistance);


        if (!IsGrounded && grounded)
        {
            //Debug.Log("Grounded");

            IsGrounded = true;

            if (isClimbing)
                isClimbing = false;

            _hasJumped = false;
            _currentMovementLerpSpeed = 100;
            OnTouchedGround?.Invoke();
            _Animator.applyRootMotion = true;
        }
        else if (IsGrounded && !grounded)
        {
            //Debug.Log("Not Grounded");
            IsGrounded = false;
            transform.SetParent(null);
            _Animator.applyRootMotion = false;
        }
    }

    #endregion

    #region Walking

    [Header("Walking")] [SerializeField] private float _walkSpeed = 4;
    [SerializeField] private float _acceleration = 2;
    [SerializeField] private float _currentMovementLerpSpeed = 100;
    [SerializeField] private float _breakSpeed = 4f;

    private void HandleWalking()
    {
        // This can be done using just X & Y input as they lerp to max values, but this gives greater control over velocity acceleration
        var acceleration = IsGrounded ? _acceleration : _acceleration * 0.5f;

        if (Input.GetKey(KeyCode.A))
        {
            if (_rb.velocity.x > 0) _inputs.X = 0; // Immediate stop and turn. Just feels better
            _inputs.X = Mathf.MoveTowards(_inputs.X, -1, acceleration * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            if (_rb.velocity.x < 0) _inputs.X = 0;
            _inputs.X = Mathf.MoveTowards(_inputs.X, 1, acceleration * Time.deltaTime);
        }
        else
        {
            _inputs.X = Mathf.MoveTowards(_inputs.X, 0, acceleration * _breakSpeed * Time.deltaTime);
        }

        var idealVel = new Vector3(_inputs.X * _walkSpeed, _rb.velocity.y);
        // _currentMovementLerpSpeed should be set to something crazy high to be effectively instant. But slowed down after a wall jump and slowly released
        _rb.velocity = Vector3.MoveTowards(_rb.velocity, idealVel, _currentMovementLerpSpeed * Time.deltaTime);

    }

    # endregion


    #region Jumping

    [Header("Jumping")] [SerializeField] private float _jumpForce = 15;
    [SerializeField] private float _fallMultiplier = 7;
    [SerializeField] private float _jumpVelocityFalloff = 8;
    //[SerializeField] private Transform _jumpLaunchPoof;
    [SerializeField] private bool _enableDoubleJump = true;
    private bool _hasJumped;
    private bool _hasDoubleJumped;

    private void HandleJumping()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log(IsGrounded);
            if (IsGrounded || _enableDoubleJump && !_hasDoubleJumped)
            {
                //Debug.Log("Has Jumped: " + _hasJumped + "Has Double Jumped" + _hasDoubleJumped);
                if (!_hasJumped || _hasJumped && !_hasDoubleJumped)
                {
                    ExecuteJump(new Vector2(_rb.velocity.x, _jumpForce), _hasJumped); // Ground jump
                    _Animator.applyRootMotion = false;
                }
                   
            }
        }

        void ExecuteJump(Vector3 dir, bool doubleJump = false)
        {
            _rb.velocity = dir;
            //_jumpLaunchPoof.up = _rb.velocity;
            _hasDoubleJumped = doubleJump;
            _hasJumped = true;
        }

        // Fall faster and allow small jumps. _jumpVelocityFalloff is the point at which we start adding extra gravity. Using 0 causes floating
        if (_rb.velocity.y < _jumpVelocityFalloff || _rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            _rb.velocity += _fallMultiplier * Physics.gravity.y * Vector3.up * Time.deltaTime;
    }
    
    #endregion

    private struct FrameInputs
    {
        public float X, Y;
        public int RawX, RawY;
    }

    #endregion

    #region Animator
    Animator _Animator;

    [SerializeField] float _RunCycleLegOffset = 0.2f;
    [SerializeField] float _AnimSpeedMultiplier = 1f;

    const float k_Half = 0.5f;
    float _TurnAmount;
    float _ForwardAmount;
    bool _Crouching;

    void UpdateAnimator(Vector3 move)
    {
        // update the animator parameters
        _Animator.SetFloat("Forward", _ForwardAmount, 0.1f, Time.deltaTime);
        _Animator.SetFloat("Turn", _TurnAmount, 0.1f, Time.deltaTime);
        _Animator.SetBool("Crouch", _Crouching);
        _Animator.SetBool("OnGround", IsGrounded);
        if (!IsGrounded)
        {
            _Animator.SetFloat("Jump", _rb.velocity.y);
        }

        // calculate which leg is behind, so as to leave that leg trailing in the jump animation
        // (This code is reliant on the specific run cycle offset in our animations,
        // and assumes one leg passes the other at the normalized clip times of 0.0 and 0.5)
        float runCycle =
            Mathf.Repeat(
                _Animator.GetCurrentAnimatorStateInfo(0).normalizedTime + _RunCycleLegOffset, 1);
        float jumpLeg = (runCycle < k_Half ? 1 : -1) * _ForwardAmount;
        if (IsGrounded)
        {
            _Animator.SetFloat("JumpLeg", jumpLeg);
        }

        // the anim speed multiplier allows the overall speed of walking/running to be tweaked in the inspector,
        // which affects the movement speed because of the root motion.
        if (IsGrounded && move.magnitude > 0)
        {
            _Animator.speed = _AnimSpeedMultiplier;
        }
        else
        {
            // don't use that while airborne
            _Animator.speed = 1;
        }
    }
    #endregion

    #region Crouching
    void ScaleCapsuleForCrouching(bool crouch)
    {
        if (IsGrounded && crouch)
        {
            if (_Crouching) return;
            _Capsule.height = _Capsule.height / 2f;
            _Capsule.center = _Capsule.center / 2f;
            _Crouching = true;
        }
        else
        {
            Ray crouchRay = new Ray(_rb.position + Vector3.up * _Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = _CapsuleHeight - _Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, _Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                _Crouching = true;
                return;
            }
            _Capsule.height = _CapsuleHeight;
            _Capsule.center = _CapsuleCenter;
            _Crouching = false;
        }
    }

    void PreventStandingInLowHeadroom()
    {
        // prevent standing up in crouch-only zones
        if (!_Crouching)
        {
            Ray crouchRay = new Ray(_rb.position + Vector3.up * _Capsule.radius * k_Half, Vector3.up);
            float crouchRayLength = _CapsuleHeight - _Capsule.radius * k_Half;
            if (Physics.SphereCast(crouchRay, _Capsule.radius * k_Half, crouchRayLength, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                _Crouching = true;
            }
        }
    }
    #endregion

    void ApplyExtraTurnRotation()
    {
        // help the character turn faster (this is in addition to root rotation in the animation)
        float turnSpeed = Mathf.Lerp(_StationaryTurnSpeed, _MovingTurnSpeed, _ForwardAmount);
        transform.Rotate(0, _TurnAmount * turnSpeed * Time.deltaTime, 0);
    }
}
