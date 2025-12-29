using TopDown.Movement;
using UnityEngine;

public class PlayerAwareness : MonoBehaviour
{
    public bool AwareOfPlayer { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }
    private float _sqrAwarenessDistance;

    // [SerializeField] private LayerMask _obstacleMask;

    [SerializeField]
    private float _playerAwarenessDistance;
    public Transform _player;
    private HealthController _playerHealth;
    private bool _playerIsDead;

    private void Awake()
    {
        //tính bình phương khoảng cách phát hiện người chơi để giảm tốn tài nguyên
        _sqrAwarenessDistance = _playerAwarenessDistance * _playerAwarenessDistance;

        var mover = FindObjectOfType<Mover>();
        if (mover != null)
        {
            _player = mover.transform;
            _playerHealth = _player.GetComponent<HealthController>();

            if (_playerHealth != null)
            {
                //khi player kích hoạt event OnDied, code sau sẽ tự chạy
                _playerHealth.OnDied.AddListener(HandlePlayerDied);
            }
        }
    }

    private void OnDestroy()
    {
        if (_playerHealth != null)
        {
            //hủy đăng ký khỏi sự kiện OnDied nếu kẻ địch này đã Destroy
            _playerHealth.OnDied.RemoveListener(HandlePlayerDied);
        }
    }

    private void HandlePlayerDied()
    {
        _playerIsDead = true;
        AwareOfPlayer = false;
        DirectionToPlayer = Vector2.zero;
    }

    private void Update()
    {
        if (_player == null || _playerIsDead)
        {
            AwareOfPlayer = false;
            DirectionToPlayer = Vector2.zero;
            return;
        }

        Vector2 enemyToPlayerVector = (Vector2)(_player.position - transform.position);
        DirectionToPlayer = enemyToPlayerVector.normalized;

        // //Kiểm tra khoảng cách trước nếu có Tường
        // if (enemyToPlayerVector.sqrMagnitude <= _sqrAwarenessDistance)
        // {
        //     // Nếu đủ gần, kiểm tra xem có tường chắn không
        //     // Bắn tia  Raycast từ Enemy, hướng về Player, độ dài bằng khoảng cách tới Player
        //     //kiểm tra va chạm với _obstacleMask
        //     RaycastHit2D hit = Physics2D.Raycast(transform.position, DirectionToPlayer, 
        //     enemyToPlayerVector.magnitude, _obstacleMask);

        //     if (hit.collider != null)
        //     {
        //         AwareOfPlayer = false; 
        //     }
        //     else
        //     {
        //         AwareOfPlayer = true;
        //     }
        // }
        // else
        // {
        //     // Quá xa
        //     AwareOfPlayer = false;
        // }

        //so sánh bình phương độ dài Vector, nếu trong tầm thì truy đuổi
        AwareOfPlayer = enemyToPlayerVector.sqrMagnitude <= _sqrAwarenessDistance;
    }

    private void OnDrawGizmosSelected()
    {
        //vòng tròn Phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _playerAwarenessDistance);

        //nếu phát hiện, vẽ 1 đường thẳng tới phía Player
        if (_player != null && !_playerIsDead)
        {
            Gizmos.color = AwareOfPlayer ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, _player.position);
        }
    }

}
