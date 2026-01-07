using System.Collections.Generic;
using Photon.Pun;
using TopDown.Audio;
using TopDown.Shooting;
using UnityEngine;

[RequireComponent(typeof(MPAwareness))]
public class MPEnemyShooting : MonoBehaviourPun
{
    [Header("References")]
    [SerializeField] private MPProjectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Animator muzzleFlashAnimator;
    [SerializeField] private AudioClip shotSound;

    [Header("Weapon Type")]
    [SerializeField] private bool isShotgun = false;
    [SerializeField] private int pelletCount = 5;

    [Header("Shooting")]
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int poolSize = 10;

    [Header("Accuracy")]
    [SerializeField] private float spreadAngle = 15f;

    [Header("Fake Bullet")]
    [SerializeField] private FakeBullet fakeBulletPrefab;

    [Header("Optional")]
    [SerializeField] private bool requireLineOfSight = false;
    [SerializeField] private LayerMask losObstacles;

    private MPAwareness awareness;
    private float fireCooldown;

    // Cached LOS data
    private int playerLayerID;
    private int playerLayerMask;

    private List<MPProjectile> realBulletPool;
    private List<FakeBullet> fakeBulletPool;

    [Header("Network Adjustment")]
    [SerializeField] private float extraLatencyBuffer = 0.05f;

    private void Awake()
    {
        awareness = GetComponent<MPAwareness>();
        playerLayerID = LayerMask.NameToLayer("Player");
        playerLayerMask = LayerMask.GetMask("Player");
        InitPool();
    }

    private void InitPool()
    {
        //Chỉ Master cần pool đạn thật để tính dame
        if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
        {
            realBulletPool = new List<MPProjectile>();
            for (int i = 0; i < poolSize; i++)
            {
                MPProjectile obj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
                obj.gameObject.SetActive(false);
                realBulletPool.Add(obj);
            }
        }

        //Tất cả Client cần pool đạn giả để hiển thị VFX
        fakeBulletPool = new List<FakeBullet>();
        for (int i = 0; i < poolSize; i++)
        {
            FakeBullet obj = Instantiate(fakeBulletPrefab, transform.position, Quaternion.identity);
            obj.gameObject.SetActive(false);
            fakeBulletPool.Add(obj);
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        if (projectilePrefab == null || firePoint == null || awareness == null)
            return;

        if (!awareness.AwareOfPlayer)
            return;

        fireCooldown -= Time.deltaTime;
        if (fireCooldown > 0f)
            return;

        // require los when ready to shoot
        if (requireLineOfSight && !HasLineOfSight())
            return;

        // Aim at player
        Vector2 dir = awareness.DirectionToPlayer.normalized;
        firePoint.up = -dir;

        Fire();
        fireCooldown = 1f / fireRate;
    }


    private bool HasLineOfSight()
    {
        Vector2 startPos = firePoint.position;
        Vector2 directionToPlayer = awareness.DirectionToPlayer.normalized;

        float maxDistance = 20f;

        LayerMask mask = losObstacles | playerLayerMask;
        RaycastHit2D hit = Physics2D.Raycast(startPos, directionToPlayer, maxDistance, mask);

        if (hit.collider != null)
        {
            bool hitPlayer = hit.collider.gameObject.layer == playerLayerID;

#if UNITY_EDITOR
            if (!hitPlayer)
            {
                Debug.Log("Bị chặn bởi: " + hit.collider.gameObject.name);
            }
            Debug.DrawRay(startPos, directionToPlayer * hit.distance, hitPlayer ? Color.green : Color.red, 0.05f);
#endif

            if (hitPlayer) return true;
        }
        else
        {
#if UNITY_EDITOR
            Debug.DrawRay(startPos, directionToPlayer * 5f, Color.yellow, 0.05f);
#endif
        }

        return false;
    }

    private void Fire()
    {
        Vector3 spawnPos = firePoint.position;
        Quaternion baseRot = firePoint.rotation;

        // Đổi từ milisecond sang second.
        float currentPing = PhotonNetwork.GetPing();
        float delaySeconds = (currentPing / 2000f) + extraLatencyBuffer;

        // Giới hạn delay tối đa để tránh địch bị lag quá lâu
        delaySeconds = Mathf.Clamp(delaySeconds, 0f, 0.4f);

        // Xác định số lượng đạn
        int bulletsToFire = isShotgun ? pelletCount : 1;

        // Tạo mảng chứa các góc lệch để gửi cho Client khác (để đồng bộ hướng bay giả)
        float[] spreadAngles = new float[bulletsToFire];

        for (int i = 0; i < bulletsToFire; i++)
        {
            // Tính góc lệch ngẫu nhiên tại Master
            float spread = Random.Range(-spreadAngle, spreadAngle);
            spreadAngles[i] = spread;

            // Tính rotation cuối cùng
            Quaternion finalRotation = baseRot * Quaternion.Euler(0, 0, spread);
            //Lấy đạn thật từ Pool
            MPProjectile proj = GetRealProjectileFromPool();
            if (proj != null)
            {
                proj.transform.position = spawnPos;
                proj.transform.rotation = finalRotation;
                proj.gameObject.SetActive(true);

                proj.OwnerID = photonView.ViewID;
                proj.ShootBullet(spawnPos, finalRotation, delaySeconds);
            }
        }

        //Báo các Client khác hiện đạn giả
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_FireVisuals), RpcTarget.Others, spawnPos, baseRot, spreadAngles);
        }

        PlayEffectsOnly();
    }

    [PunRPC]
    private void RPC_FireVisuals(Vector3 spawnPos, Quaternion baseRot, float[] spreadAngles)
    {
        // Duyệt qua danh sách góc lệch nhận được từ Master
        foreach (float spread in spreadAngles)
        {
            Quaternion finalRotation = baseRot * Quaternion.Euler(0, 0, spread);

            // Spawn đạn giả
            FakeBullet fake = GetFakeBulletFromPool();
            if (fake != null)
            {
                fake.transform.position = spawnPos;
                fake.transform.rotation = finalRotation;
                fake.SetOwner(gameObject);
                fake.gameObject.SetActive(true);
            }
        }

        PlayEffectsOnly();
    }

    private void PlayEffectsOnly()
    {
        if (muzzleFlashAnimator != null)
            muzzleFlashAnimator.SetTrigger("shoot");

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(shotSound);
    }

    private MPProjectile GetRealProjectileFromPool()
    {
        // Nếu pool chưa được tạo (do lúc Init chưa phải Master), thì tạo mới ngay
        if (realBulletPool == null)
        {
            realBulletPool = new List<MPProjectile>();
        }

        foreach (var p in realBulletPool)
        {
            if (!p.gameObject.activeInHierarchy) return p;
        }
        //hết pool thì tạo mới
        var newObj = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        newObj.gameObject.SetActive(false);
        realBulletPool.Add(newObj);
        return newObj;
    }

    private FakeBullet GetFakeBulletFromPool()
    {
        foreach (var p in fakeBulletPool)
        {
            if (!p.gameObject.activeInHierarchy) return p;
        }

        var newObj = Instantiate(fakeBulletPrefab, transform.position, Quaternion.identity);
        fakeBulletPool.Add(newObj);
        return newObj;
    }
}
