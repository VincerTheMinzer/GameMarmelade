using System;
using UnityEngine;

public class MySpecialMovement : MonoBehaviour
{
    private Rigidbody _rb;
    public GameObject MyCameraGO;
    public Transform MyCameraTrans;

    //[SerializeField] private Animator _anim;

    //MOVEMENT
    [Header("Movement Variables")]
    

    // ----------
    private FrameInputs _inputs;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleGrounding();

        HandleWalking();

        HandleJumping();

        Debug.Log(_inputs.X + ", " + _rb.velocity + ", " + IsGrounded);
        //ApplySpeed();
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

        //Debug.Log("grounded: " + grounded);
        //Debug.Log("IsGrounded: " + IsGrounded);

        if (!IsGrounded && grounded)
        {
            //Debug.Log("Grounded");

            IsGrounded = true;
            _hasJumped = false;
            _currentMovementLerpSpeed = 100;
            OnTouchedGround?.Invoke();
        }
        else if (IsGrounded && !grounded)
        {
            //Debug.Log("Not Grounded");
            IsGrounded = false;
            transform.SetParent(null);
        }
    }

    #endregion

    #region Jumping

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
                if (!_hasJumped || _hasJumped && !_hasDoubleJumped) ExecuteJump(new Vector2(_rb.velocity.x, _jumpForce), _hasJumped); // Ground jump
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

    /*
    #region Vincent
    public float maxSpeed = 5;
    public float AddSpeed = 0.5f;
    public float breakSpeed = 0.2f;
    float curSpeed = 0;

    private float breakpercent = 0;

   
    Vector3 controllerInput = Vector3.zero;
    Vector3 moveDir = Vector3.zero;
    Vector3 curMoveDir = Vector3.zero;
    bool hadPlayerInput = false;
    bool setupcomplete = false;

    private void checkInput()
    {
        hadPlayerInput = false;

        // Move right direction
        if (Input.GetKey(KeyCode.D))
        {
            if (_rb.velocity.x < 0)

                controllerInput += transform.forward * (AddSpeed * Time.deltaTime);
            hadPlayerInput = true;
        }
        // Move left direction
        else if (Input.GetKey(KeyCode.A))
        {
            if (_rb.velocity.x < 0)

                controllerInput -= transform.forward * (AddSpeed * Time.deltaTime);
            hadPlayerInput = true;
        }
    }

    private void ApplySpeed()
    {
        if (!hadPlayerInput)
        {
            if(!setupcomplete)
                moveDir = controllerInput;

            curMoveDir = Vector3.Lerp(moveDir, Vector3.zero, Mathf.Clamp(breakpercent += (breakSpeed * Time.deltaTime), 0, 1));
            controllerInput = curMoveDir;
            setupcomplete = true;
            curSpeed -= breakSpeed;
        }
        else
        {
            setupcomplete = false;
            breakpercent = 0;
            curSpeed += AddSpeed;
            curMoveDir = controllerInput;
        }

        _rb.velocity = curMoveDir;
    }
    #endregion
    */

}
