using UnityEngine;

public class VehicleMovement : MonoBehaviour
{
    public struct TargetTransformState
    {
        public Vector3 Forward;
        public Vector3 Right;
        public Vector3 Up;

        public Vector3 Position;
        public Quaternion Rotation;
    }

	public float Speed { get; private set; }						//The current forward speed of the ship

	[Header("Drive Settings")]
	[SerializeField] private float _driveForce = 17f;           //The force that the engine generates

    [SerializeField] private float _slowingVelFactor = .99f;   //The percentage of velocity the ship maintains when not thrusting (e.g., a value of .99 means the ship loses 1% velocity when not thrusting)
    [SerializeField] private float _brakingVelFactor = .95f;   //The percentage of velocty the ship maintains when braking
    [SerializeField] private float _angleOfRoll = 30f;			//The angle that the ship "banks" into a turn

	[Header("Hover Settings")]
	[SerializeField] private float _hoverHeight = 1f;        //The height the ship maintains when hovering
    [SerializeField] private float _hoverForce = 300f;            //The force of the ship's hovering
    [SerializeField] private LayerMask _wallLayerMask;
    [SerializeField] private PIDController _hoverPID;			//A PID controller to smooth the ship's hovering

	[Header("Physics Settings")]
	[SerializeField] private Transform _shipBody;				//A reference to the ship's body, this is for cosmetics
	[SerializeField] private float _terminalVelocity = 100f;   //The max speed the ship can go
    [SerializeField] private float _hoverGravity = 20f;        //The gravity applied to the ship while it is on the ground
    [SerializeField] private float _fallGravity = 60f;			//The gravity applied to the ship while it is falling

    // [SerializeField] private float _pitchAdjustementSpeed = 360f;

    [Range(0f, 1f)]
    [Tooltip("1 means no drifting, 0 means no side friction")]
    [SerializeField] private float _sideFrictionMultiplier = 1f;

    [SerializeField] private Vector2 _rollSpeedMultiplier = new Vector2(1.5f, 0.5f);
    [SerializeField] private Vector2 _rotationSpeedMultiplierRange = new Vector2(4f, 1f);
    
    private BoostingController _booster;
    private Stabilizers _stabilizers;

    private Rigidbody _rigidBody;
	private PlayerInput _input;			
	private float _drag;
	private bool _isOnGround;

    private TargetTransformState _currentTargetState;
    
    private void Start()
	{
		//Get references to the Rigidbody and PlayerInput components
		_rigidBody = GetComponent<Rigidbody>();
		_input = GetComponent<PlayerInput>();
	    _booster = GetComponent<BoostingController>();

	    _stabilizers = GetComponent<Stabilizers>();

		//Calculate the ship's drag value
		_drag = _driveForce / _terminalVelocity;

	    _isOnGround = _stabilizers.Raycast(out _lastInfo);
    }

    private void FixedUpdate()
	{
		Speed = Vector3.Dot(_rigidBody.velocity, transform.forward);
        
        CalculatHover();
        CalculatePropulsion();
	}

    private Stabilizers.StabilizerInfo _lastInfo;

    private void Update()
    {
        _isOnGround = _stabilizers.Raycast(out _lastInfo);
        
        //Calculate the angle we want the ship's body to bank into a turn based on the current rudder
        float angle = Mathf.Lerp(_rollSpeedMultiplier.x, _rollSpeedMultiplier.y, GetSpeedPercentage()) * _angleOfRoll * -_input.Rudder;

        Quaternion bodyRotation = _currentTargetState.Rotation * Quaternion.Euler(0f, 0f, angle);
        _shipBody.rotation = Quaternion.Lerp(_shipBody.rotation, bodyRotation, Time.deltaTime * 10f);
    }

    private void CalculatHover()
	{
		Vector3 groundNormal;

	    _currentTargetState.Position = _rigidBody.position;

        // Stabilizers.StabilizerInfo _lastInfo;
	    //_isOnGround = _stabilizers.Raycast(out stabilizerInfo);

        Vector3 force;

		if (_isOnGround)
		{
			//...determine how high off the ground it is...
			float height = _lastInfo.Distance;
			//...save the normal of the ground...
			groundNormal = _lastInfo.Normal;
			//...use the PID controller to determine the amount of hover force needed...
			float forcePercent = _hoverPID.Seek(_hoverHeight, height);
			
			//...calulcate the total amount of hover force based on normal (or "up") of the ground...
			force = groundNormal * (_hoverForce * forcePercent - _hoverGravity * height);
		}
		else
		{
			//...use Up to represent the "ground normal". This will cause our ship to
			//self-right itself in a case where it flips over
		    groundNormal = _lastInfo.Normal;
			// Calculate and apply the stronger falling gravity straight down on the ship
			force = -groundNormal * _fallGravity;
		}

		//Calculate the amount of pitch and roll the ship needs to match its orientation
		//with that of the ground. This is done by creating a projection and then calculating
		//the rotation needed to face that projection
		Vector3 projection = Vector3.ProjectOnPlane(_rigidBody.rotation * Vector3.forward, groundNormal);
        
	    Quaternion rotation = Quaternion.LookRotation(projection, groundNormal);

        _currentTargetState.Rotation = rotation;
	    _currentTargetState.Forward = rotation * Vector3.forward;
	    _currentTargetState.Right = rotation * Vector3.right;
	    _currentTargetState.Up = rotation * Vector3.up;
        
	    //Move the ship over time to match the desired rotation to match the ground

	    // var targetRotation = Quaternion.RotateTowards(_rigidBody.rotation, rotation, Time.deltaTime * _pitchAdjustementSpeed);

	    _rigidBody.AddForce(force, ForceMode.Acceleration);
        _rigidBody.MoveRotation(rotation);
	}

    private void CalculatePropulsion()
    {
        float rotationTorque =
            Mathf.Lerp(_rotationSpeedMultiplierRange.x, _rotationSpeedMultiplierRange.y, GetSpeedPercentage()) *
            _input.Rudder;

        _rigidBody.AddRelativeTorque(0f, rotationTorque, 0f, ForceMode.VelocityChange);
        
		float sidewaysSpeed = Vector3.Dot(_rigidBody.velocity, _currentTargetState.Right);

		//Calculate the desired amount of friction to apply to the side of the vehicle. This
		//is what keeps the ship from drifting into the walls during turns. If you want to add
		//drifting to the game, divide Time.fixedDeltaTime by some amount
		Vector3 sideFriction = -_currentTargetState.Right * (sidewaysSpeed / Time.fixedDeltaTime) * _sideFrictionMultiplier; 
        _rigidBody.AddForce(sideFriction, ForceMode.Acceleration);

		//If not propelling the ship, slow the ships velocity
		if (_input.Thruster <= 0f)
			_rigidBody.velocity *= _slowingVelFactor;

	    if (_booster.CurrentBoostPower > 0f)
	    {
	        float boost = _driveForce * _booster.CurrentBoostPower;
	        _rigidBody.AddForce(_currentTargetState.Forward * boost, ForceMode.Acceleration);
        }

        if (!_isOnGround)
            return;

        //If the ship is braking, apply the braking velocty reduction
        if (_input.IsBraking)
			_rigidBody.velocity *= _brakingVelFactor;

		//Calculate and apply the amount of propulsion force by multiplying the drive force
		//by the amount of applied thruster and subtracting the drag amount
		float propulsion = _driveForce * _input.Thruster - _drag * Mathf.Clamp(Speed, 0f, _terminalVelocity);
		_rigidBody.AddForce(_currentTargetState.Forward * propulsion, ForceMode.Acceleration);
	}

	public float GetSpeedPercentage()
	{
		//Returns the total percentage of speed the ship is traveling
		return _rigidBody.velocity.magnitude / _terminalVelocity;
	}

    private void OnCollisionStay(Collision collision)
    {
        //If the ship has collided with an object on the Wall layer...
        if ((collision.gameObject.layer & _wallLayerMask.value) != 0)
        {
            //...calculate how much upward impulse is generated and then push the vehicle down by that amount 
            //to keep it stuck on the track (instead up popping up over the wall)
            Vector3 upwardForceFromCollision = Vector3.Dot(collision.impulse, _currentTargetState.Up) * -_currentTargetState.Up;
            _rigidBody.AddForce(-upwardForceFromCollision, ForceMode.Impulse);
        }
    }
}
