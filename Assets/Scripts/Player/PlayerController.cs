using UnityEngine;
using UnityEngine.Playables;
using World;

// Simple player controller that can move only on three lanes (Left, Center, Right)
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float laneChangeSpeed = 10f;

    [Header("Jump")]
    [Tooltip("Initial upward velocity applied when jumping. Tune this value to adjust jump height.")]
    [SerializeField] private float jumpVelocity = 6f;
    [Tooltip("Gravity applied to the player (negative value). Tune to change fall speed.")]
    [SerializeField] private float gravity = -20f;

    private readonly float[] _lanePositions = new float[3];
    private int _currentLaneIndex = 1; //0 = Left,1 = Center,2 = Right

    private Vector3 _targetPosition;
    private GameController _gameController;

    // jumping state
    private float _verticalVelocity = 0f;
    private bool _isGrounded = true;
    private float _groundY;

    private void Start()
    {
        _gameController = GameController.Instance;
 
        // initialize lane positions from serialized values
        _lanePositions[0] = _gameController.WorldManager.GetLaneXPosition(LaneNumber.Left);
        _lanePositions[1] = _gameController.WorldManager.GetLaneXPosition(LaneNumber.Center);
        _lanePositions[2] = _gameController.WorldManager.GetLaneXPosition(LaneNumber.Right);

        // set initial target position to current lane's x
        _targetPosition = new Vector3(_lanePositions[_currentLaneIndex], transform.position.y, transform.position.z);

        // snap player to center lane on start
        transform.position = new Vector3(_lanePositions[_currentLaneIndex], transform.position.y, transform.position.z);

        // record ground Y (assume starting on ground)
        _groundY = transform.position.y;
        _isGrounded = true;
        _verticalVelocity = 0f;
    }

    private void Update()
    {
        HandleInput();
        SmoothMoveToTargetLane();
        ApplyGravityAndJump();
    }

    private void HandleInput()
    {
        // move left
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            TryChangeLane(-1);
        }

        // move right
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            TryChangeLane(1);
        }

        // jump (Up arrow or W)
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            TryJump();
        }
    }

    private void TryChangeLane(int direction)
    {
        int desired = Mathf.Clamp(_currentLaneIndex + direction, 0, 2);
        if (desired == _currentLaneIndex) return;

        _currentLaneIndex = desired;
        _targetPosition = new Vector3(_lanePositions[_currentLaneIndex], transform.position.y, transform.position.z);
    }

    private void SmoothMoveToTargetLane()
    {
        Vector3 newPos = transform.position;
        newPos.x = Mathf.MoveTowards(transform.position.x, _targetPosition.x, laneChangeSpeed * Time.deltaTime);
        // preserve current vertical position (jumping) and z
        newPos.y = transform.position.y;
        newPos.z = transform.position.z;
        transform.position = newPos;
    }

    private void TryJump()
    {
        if (_isGrounded)
        {
            _verticalVelocity = jumpVelocity;
            _isGrounded = false;
        }
    }

    private void ApplyGravityAndJump()
    {
        // apply gravity
        _verticalVelocity += gravity * Time.deltaTime;

        float newY = transform.position.y + _verticalVelocity * Time.deltaTime;

        if (newY <= _groundY)
        {
            newY = _groundY;
            _verticalVelocity = 0f;
            _isGrounded = true;
        }

        Vector3 p = transform.position;
        p.y = newY;
        transform.position = p;
    }

    // Optional: allow external code to set lane x positions (for example from WorldChunk)
    public void SetLanePositions(float left, float center, float right)
    {
        _lanePositions[0] = left;
        _lanePositions[1] = center;
        _lanePositions[2] = right;
        // update target and snap current x if needed
        _targetPosition.x = _lanePositions[_currentLaneIndex];
    }

    public LaneNumber GetCurrentLane()
    {
        return _currentLaneIndex == 0 ? LaneNumber.Left : (_currentLaneIndex == 1 ? LaneNumber.Center : LaneNumber.Right);
    }
}
