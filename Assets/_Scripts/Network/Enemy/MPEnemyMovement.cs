using System.Collections;
using Photon.Pun;
using UnityEngine;
public class MPEnemyMovement : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private GameObject minimapIcon;
    [SerializeField] private float minChaseDistance = 1f;

    private Rigidbody2D _rigidbody;
    private MPAwareness _playerAwareness;
    private Vector2 _targetDirection;
    private Animator _animator;

    public bool _isDead;
    public enum PatrolMode { None, Horizontal, Vertical }

    [Header("Patrol")]
    [SerializeField] private PatrolMode patrolMode = PatrolMode.None;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private bool pauseAtEnds = false;
    [SerializeField] private float pauseDuration = 0.5f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float obstacleDetectRange = 0.5f; // Raycast distance
    [SerializeField] private float obstaclePauseDuration = 0.25f;
    private bool _isObstaclePause;
    private bool _isRegistered;

    // Patrol state
    private Vector2 _startPos;
    private Vector2 _patrolA;
    private Vector2 _patrolB;
    private Vector2 _currentPatrolTarget;
    private float _pauseTimer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwareness = GetComponent<MPAwareness>();
        _animator = GetComponent<Animator>();
        _targetDirection = transform.up;

        _startPos = transform.position;

        if (patrolMode == PatrolMode.Horizontal)
        {
            _patrolA = _startPos + Vector2.left * patrolDistance / 10;
            _patrolB = _startPos + Vector2.right * patrolDistance / 10;
        }
        else if (patrolMode == PatrolMode.Vertical)
        {
            _patrolA = _startPos + Vector2.down * patrolDistance / 10;
            _patrolB = _startPos + Vector2.up * patrolDistance / 10;
        }

        _currentPatrolTarget = _patrolB;
    }

    // private IEnumerator Start()
    // {
    //     // wait until Instance != null
    //     while (MPEnemyManager.Instance == null ||
    //        !PhotonNetwork.IsConnected ||
    //        !PhotonNetwork.InRoom ||
    //        !PhotonNetwork.IsMasterClient)
    //     {
    //         yield return null;
    //     }

    //     if (_isRegistered) yield break;

    //     _isRegistered = true;
    //     MPEnemyManager.Instance.RegisterEnemy();
    // }

    private void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        if (_isDead) return;

        DetectObstacle();
        UpdateTargetDirection();
        RotateTowardTarget();
        SetVelocity();
        UpdateAnimation();
    }

    private void UpdateTargetDirection()
    {
        if (_isDead) return;
        if (_playerAwareness != null && _playerAwareness.AwareOfPlayer)
        {
            _targetDirection = _playerAwareness.DirectionToPlayer;
            return;
        }

        //phát hiện vật cản thì dừng tới khi hết bộ đếm
        if (_isObstaclePause)
        {
            _pauseTimer -= Time.deltaTime;
            _targetDirection = Vector2.zero;

            if (_pauseTimer <= 0f)
                _isObstaclePause = false;

            return;
        }

        if (patrolMode == PatrolMode.None)
        {
            _targetDirection = Vector2.zero;
            _animator.SetBool("moving", false);
            return;
        }

        //đợi trước khi xoay người
        if (pauseAtEnds && _pauseTimer > 0f)
        {
            _pauseTimer -= Time.deltaTime;
            _targetDirection = Vector2.zero;
            return;
        }

        Vector2 pos = _rigidbody.position;
        Vector2 toTarget = _currentPatrolTarget - pos;

        const float arriveThreshold = 0.05f;
        if (toTarget.sqrMagnitude <= arriveThreshold * arriveThreshold)
        {
            _currentPatrolTarget = (_currentPatrolTarget == _patrolA) ? _patrolB : _patrolA;
            if (pauseAtEnds) _pauseTimer = pauseDuration;
            toTarget = _currentPatrolTarget - pos;
        }

        _targetDirection = toTarget.normalized;
    }

    private void DetectObstacle()
    {
        if (_playerAwareness.AwareOfPlayer) return;
        if (_targetDirection == Vector2.zero) return;
        if (_isObstaclePause) return;

        Vector2 origin = _rigidbody.position;
        Vector2 direction = transform.up;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, obstacleDetectRange, obstacleLayer);

        Color color = hit ? Color.red : Color.green;
        Debug.DrawRay(origin, direction * obstacleDetectRange, color);

        if (hit.collider != null)
        {
            _targetDirection = Vector2.zero;
            _rigidbody.linearVelocity = Vector2.zero;

            _currentPatrolTarget = (_currentPatrolTarget == _patrolA) ? _patrolB : _patrolA;
            _targetDirection = -transform.up;

            _isObstaclePause = true;
            _pauseTimer = obstaclePauseDuration;
        }
    }

    private void RotateTowardTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, _targetDirection);
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        _rigidbody.SetRotation(rotation);
    }

    private void SetVelocity()
    {
        if (_targetDirection == Vector2.zero || _isObstaclePause)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (_playerAwareness.AwareOfPlayer)
        {
            float dist = Vector2.Distance(transform.position, _playerAwareness.currentTarget.position);

            if (dist < minChaseDistance)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _targetDirection = Vector2.zero;
                return;
            }
        }

        _rigidbody.linearVelocity = transform.up * _speed;
    }

    private void UpdateAnimation()
    {
        if (_isDead) return;
        bool isMoving = _targetDirection != Vector2.zero;
        _animator.SetBool("moving", isMoving);
    }

    public void PlayDeathAnimation()
    {
        if (_isDead) return;
        _isDead = true;

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        _animator.SetBool("moving", false);
        _animator.SetTrigger("IsDead");

        transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y,
            transform.eulerAngles.z - 90f
        );

        Destroy(minimapIcon);
        MPEnemyManager.Instance?.OnEnemyDied();
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolMode == PatrolMode.None) return;

        Vector2 origin = Application.isPlaying ? _startPos : (Vector2)transform.position;
        Vector2 a = origin;
        Vector2 b = origin;

        if (patrolMode == PatrolMode.Horizontal)
        {
            a = origin + Vector2.left * patrolDistance / 10;
            b = origin + Vector2.right * patrolDistance / 10;
        }
        else if (patrolMode == PatrolMode.Vertical)
        {
            a = origin + Vector2.down * patrolDistance / 10;
            b = origin + Vector2.up * patrolDistance / 10;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawSphere(a, 0.07f);
        Gizmos.DrawSphere(b, 0.07f);
    }
}
