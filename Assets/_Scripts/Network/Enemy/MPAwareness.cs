using ExitGames.Client.Photon.StructWrapping;
using TopDown.Movement;
using UnityEngine;

public class MPAwareness : MonoBehaviour
{
    public bool AwareOfPlayer { get; private set; }
    public Vector2 DirectionToPlayer { get; private set; }

    [SerializeField] private float _playerAwarenessDistance = 5f;
    [SerializeField] private float camouflageMultiplier = 0.5f;

    public Transform currentTarget;
    private MPHealthController currentTargetHealth;
    private bool targetIsDead;

    // Thay vì gọi mỗi frame, dùng timer để tăng hiệu năng
    private float _findTargetCooldown = 0.2f;
    private float _lastFindTime;

    private void Update()
    {
        if (Time.time > _lastFindTime + _findTargetCooldown)
        {
            FindClosestAlivePlayer();
            _lastFindTime = Time.time;
        }

        if (currentTarget == null || targetIsDead)
        {
            AwareOfPlayer = false;
            DirectionToPlayer = Vector2.zero;
            return;
        }
        Vector2 v = (Vector2)(currentTarget.position - transform.position);
        DirectionToPlayer = v.normalized;

        float reducedDistance = _playerAwarenessDistance;

        if (currentTarget.gameObject.name.StartsWith("Player3"))
        {
            reducedDistance *= camouflageMultiplier;
        }
        AwareOfPlayer = v.magnitude <= reducedDistance;
    }

    private void FindClosestAlivePlayer()
    {
        Mover[] players = FindObjectsOfType<Mover>();

        if (players == null || players.Length == 0)
        {
            currentTarget = null;
            return;
        }

        Mover closestPlayer = null;
        float closestDistance = Mathf.Infinity;

        //duyệt qua tất cả Players
        foreach (Mover player in players)
        {
            float dist = Vector2.Distance(transform.position, player.transform.position);

            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = player;
            }
        }

        if (closestPlayer == null)
        {
            currentTarget = null;
            return;
        }

        // nếu đổi sang Player khác, update lắng nghe OnDied của nó
        if (currentTarget != closestPlayer.transform)
        {
            if (currentTargetHealth != null)
                currentTargetHealth.OnDied.RemoveListener(HandleTargetDied);

            currentTarget = closestPlayer.transform;
            currentTargetHealth = currentTarget.GetComponent<MPHealthController>();

            targetIsDead = false;

            if (currentTargetHealth != null)
                currentTargetHealth.OnDied.AddListener(HandleTargetDied);
        }
    }


    private void HandleTargetDied()
    {
        targetIsDead = true;
        AwareOfPlayer = false;
        DirectionToPlayer = Vector2.zero;
    }

    private void OnDestroy()
    {
        if (currentTargetHealth != null)
            currentTargetHealth.OnDied.RemoveListener(HandleTargetDied);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = AwareOfPlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _playerAwarenessDistance);

        if (currentTarget != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, currentTarget.position);
        }
    }
}
