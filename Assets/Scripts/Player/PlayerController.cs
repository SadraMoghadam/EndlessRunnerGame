using UnityEngine;
using UnityEngine.Playables;
using World;

// Simple player controller that can move only on three lanes (Left, Center, Right)
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float laneChangeSpeed = 10f;

    private readonly float[] _lanePositions = new float[3];
    private int _currentLaneIndex = 1; //0 = Left,1 = Center,2 = Right

    private Vector3 _targetPosition;
    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameManager.Instance;
 
        // initialize lane positions from serialized values
        _lanePositions[0] = _gameManager.WorldManager.GetLaneXPosition(LaneNumber.Left);
        _lanePositions[1] = _gameManager.WorldManager.GetLaneXPosition(LaneNumber.Center);
        _lanePositions[2] = _gameManager.WorldManager.GetLaneXPosition(LaneNumber.Right);

        // set initial target position to current lane's x
        _targetPosition = new Vector3(_lanePositions[_currentLaneIndex], transform.position.y, transform.position.z);

        // snap player to center lane on start
        transform.position = new Vector3(_lanePositions[_currentLaneIndex], transform.position.y, transform.position.z);
    }

    private void Update()
    {
        HandleInput();
        SmoothMoveToTargetLane();
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
        transform.position = newPos;
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
