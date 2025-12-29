using UnityEngine;

public class BossEnemyMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _rotationSpeed = 720f;

    [Header("Path Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private bool startAtPointA = true;
    [SerializeField] private float waitAtPoint = 1f; // time to pause at A/B

    [Header("Misc")]
    [SerializeField] private GameObject minimapIcon;

    private PlayerAwareness _playerAwareness;

    private Rigidbody2D _rigidbody;
    private Animator _animator;

    private Vector2 _targetDirection;
    private Transform _currentTarget;
    private float _waitTimer;
    private bool _isDead;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _playerAwareness = GetComponent<PlayerAwareness>();

        //đặt boss vào vị trí đầu
        if (startAtPointA && pointA != null)
        {
            transform.position = pointA.position;
            _currentTarget = pointB;
        }
        else if (!startAtPointA && pointB != null)
        {
            transform.position = pointB.position;
            _currentTarget = pointA;
        }
        else
        {
            _currentTarget = pointB != null ? pointB : pointA;
        }
    }

    private void Start()
    {
        EnemyManager.Instance.RegisterEnemy();
    }

    private void FixedUpdate()
    {
        if (_isDead) return;

        UpdateMovement();
        RotateTowardTarget();
        SetVelocity();
        UpdateAnimation();
    }

    private void UpdateMovement()
    {
        //đứng yên nếu chưa đặt điểm
        if (pointA == null || pointB == null || _currentTarget == null)
        {
            _targetDirection = Vector2.zero;
            return;
        }

        //dừng 1 lúc ở cuối điểm
        if (_waitTimer > 0f)
        {
            _waitTimer -= Time.deltaTime;
            _targetDirection = Vector2.zero;
            return;
        }

        Vector2 pos = _rigidbody.position;
        Vector2 targetPos = _currentTarget.position;
        Vector2 toTarget = targetPos - pos;

        //nếu gần tới đích, đổi hướng và bắt đầu bộ đếm
        float arriveThresholdSqr = 0.05f * 0.05f;

        if (toTarget.sqrMagnitude <= arriveThresholdSqr)
        {
            _currentTarget = (_currentTarget == pointA) ? pointB : pointA;
            _waitTimer = waitAtPoint;
            _targetDirection = Vector2.zero;
        }
        //chưa thì đi tiếp
        else
        {
            _targetDirection = toTarget.normalized;
        }
    }

    private void RotateTowardTarget()
    {
        Vector2 lookDir = _targetDirection; //nhìn điểm A hoặc B nếu chưa phát hiện Player

        //nếu phát hiện Player, xoay mặt về nó
        if (_playerAwareness != null && _playerAwareness.AwareOfPlayer &&
            _playerAwareness.DirectionToPlayer != Vector2.zero)
        {
            lookDir = _playerAwareness.DirectionToPlayer;
        }

        if (lookDir == Vector2.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(
            transform.forward,
            lookDir
        );

        Quaternion rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            _rotationSpeed * Time.deltaTime
        );

        _rigidbody.SetRotation(rotation);
    }


    private void SetVelocity()
    {
        if (_targetDirection == Vector2.zero)
        {
            _rigidbody.linearVelocity = Vector2.zero;
            return;
        }

        _rigidbody.linearVelocity = _targetDirection * _speed;
    }

    private void UpdateAnimation()
    {
        if (_isDead) return;

        bool isMoving = _targetDirection != Vector2.zero;
        if (_animator != null)
        {
            _animator.SetBool("moving", isMoving);
        }
    }

    public void PlayDeathAnimation()
    {
        if (_isDead) return;
        _isDead = true;

        _rigidbody.linearVelocity = Vector2.zero;
        _rigidbody.angularVelocity = 0f;

        if (_animator != null)
        {
            _animator.SetBool("moving", false);
            _animator.SetTrigger("IsDead");
        }

        // Rotate -90 degrees
        transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x,
            transform.eulerAngles.y,
            transform.eulerAngles.z - 90f
        );

        if (minimapIcon != null)
        {
            Destroy(minimapIcon);
        }

        EnemyManager.Instance?.OnEnemyDied();
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA == null || pointB == null) return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(pointA.position, pointB.position);
        Gizmos.DrawSphere(pointA.position, 0.07f);
        Gizmos.DrawSphere(pointB.position, 0.07f);
    }
}
