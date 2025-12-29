using UnityEngine;
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private GameObject minimapIcon;
    [SerializeField] private float minChaseDistance = 1f;

    private Rigidbody2D _rigidbody;
    private PlayerAwareness _playerAwareness;
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

    private Vector2 _startPos;
    private Vector2 _patrolA;
    private Vector2 _patrolB;
    private Vector2 _currentPatrolTarget;
    private float _pauseTimer;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _playerAwareness = GetComponent<PlayerAwareness>();
        _animator = GetComponent<Animator>();
        _targetDirection = transform.up;

        //Lưu vị trí ban đầu để tính toán
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

        _currentPatrolTarget = _patrolB; //đặt mục tiêu di chuyển tới điểm B 
    }

    private void Start()
    {
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RegisterEnemy();
        }
        else
        {
            Debug.LogError("EnemyManager.Instance is still null!");
        }
    }

    private void FixedUpdate()
    {
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

        //nếu chạm vật cản, đứng yên 1 tí
        if (_isObstaclePause)
        {
            _pauseTimer -= Time.deltaTime; //giảm thời gian chờ
            _targetDirection = Vector2.zero;

            if (_pauseTimer <= 0f)
                _isObstaclePause = false;

            return; //không tuần tra nữa nếu đang đứng yên
        }

        if (patrolMode == PatrolMode.None)
        {
            _targetDirection = Vector2.zero;
            _animator.SetBool("moving", false);
            return;
        }

        // tạm dừng khi chạm đích
        if (pauseAtEnds && _pauseTimer > 0f)
        {
            _pauseTimer -= Time.deltaTime;
            _targetDirection = Vector2.zero;
            return;
        }

        //Logic di chuyển qua lại giữa 2 điểm
        Vector2 pos = _rigidbody.position;
        Vector2 toTarget = _currentPatrolTarget - pos;

        //kiếm tra gần tới mục tiêu chưa
        const float arriveThreshold = 0.05f;

        if (toTarget.sqrMagnitude <= arriveThreshold * arriveThreshold)
        {
            //đổi chiều
            _currentPatrolTarget = (_currentPatrolTarget == _patrolA) ? _patrolB : _patrolA;
            if (pauseAtEnds) _pauseTimer = pauseDuration;

            //tính lại hướng sau khi đổi mục tiêu
            toTarget = _currentPatrolTarget - pos;
        }
        _targetDirection = toTarget.normalized;
    }

    private void DetectObstacle()
    {
        if (_playerAwareness.AwareOfPlayer) return;
        if (_targetDirection == Vector2.zero) return;
        if (_isObstaclePause) return;

        //Thiết lập cho tia Raycast
        Vector2 origin = _rigidbody.position;
        Vector2 direction = transform.up; // hướng về phía trước mặt

        // bắn tia raycast
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, obstacleDetectRange, obstacleLayer);

        Color color = hit ? Color.red : Color.green;
        Debug.DrawRay(origin, direction * obstacleDetectRange, color);

        if (hit.collider != null)
        {
            //dừng di chuyển
            _targetDirection = Vector2.zero;
            _rigidbody.linearVelocity = Vector2.zero;

            //đổi mục tiêu ngay
            _currentPatrolTarget = (_currentPatrolTarget == _patrolA) ? _patrolB : _patrolA;
            transform.rotation = Quaternion.Euler(0f, 0f, transform.eulerAngles.z + 180f);

            //chờ 1 tí tránh giật
            _isObstaclePause = true;
            _pauseTimer = obstaclePauseDuration;
        }
    }

    private void RotateTowardTarget()
    {
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward, _targetDirection);
        //xoay từ từ, mỗi frame chỉ xoay tối đa (_rotationSpeed * Time.deltaTime)
        Quaternion rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);

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
            float dist = Vector2.Distance(transform.position, _playerAwareness._player.position);

            if (dist < minChaseDistance)
            {
                _rigidbody.linearVelocity = Vector2.zero;
                _targetDirection = Vector2.zero;
                return;
            }
        }

        _rigidbody.linearVelocity = transform.up * _speed * 80f * Time.fixedDeltaTime;
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

        //ngưng di chuyển khi chết
        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;
        _animator.SetBool("moving", false);
        _animator.SetTrigger("IsDead");

        // xoay 90 độ để đúng với Sprite
        transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y,
            transform.eulerAngles.z - 90f
        );
        Destroy(minimapIcon);

        //Báo cho manager
        EnemyManager.Instance?.OnEnemyDied();
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolMode == PatrolMode.None) return;

        // Draw patrol path
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
