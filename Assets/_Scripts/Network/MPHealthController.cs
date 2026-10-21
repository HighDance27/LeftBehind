using System.Collections;
using TopDown.Audio;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

public class MPHealthController : MonoBehaviourPun
{
    [SerializeField] private bool isEnemy = true;
    public bool IsEnemy => isEnemy;

    [Header("Health Settings")]
    [SerializeField] private int currentHealth;
    [SerializeField] private int maximumHealth;

    [Header("Armor Settings")]
    [SerializeField] private int currentArmor;
    [SerializeField] private int maximumArmor;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip dieSound;
    [SerializeField] public AudioClip wallSFX;
    [SerializeField] private ParticleSystem explodeVFXPrefab;

    private bool hasDied = false;
    private bool triggersGameOver;

    private PlayerHUD playerHUD;    //local player

    public bool IsInvincible { get; set; }

    [Header("Events")]
    public UnityEvent OnDied;
    public UnityEvent OnDamaged;
    public UnityEvent OnHealthChanged;
    public UnityEvent OnArmorChanged;

    public int CurrentHealth => currentHealth;
    public int MaximumHealth => maximumHealth;
    public int CurrentArmor => currentArmor;

    public float RemainingHealthPercentage
    {
        get
        {
            return currentHealth / (float)maximumHealth; //Parse to update HP UI
        }
    }

    public float RemainingArmorPercentage
    {
        get
        {
            if (maximumArmor == 0) return 0;
            return currentArmor / (float)maximumArmor;
        }
    }

    private void Awake()
    {
        triggersGameOver = CompareTag("Player");

        if (triggersGameOver)
        {
            playerHUD = FindObjectOfType<PlayerHUD>();
        }
    }

    private void OnEnable()
    {
        // Khi Player được spawn, tự thêm mình vào danh sách
        if (CompareTag("Player"))
        {
            SpawnPlayers.ActivePlayers.Add(this);
        }
    }

    private void OnDisable()
    {
        // Khi Player chết, tự xóa mình khỏi danh sách
        if (CompareTag("Player"))
        {
            SpawnPlayers.ActivePlayers.Remove(this);
        }
    }

    [PunRPC]
<<<<<<< Updated upstream:Assets/_Scripts/Network/MPHealth.cs
    private void RPC_TakeDamage(int damage, int killerActorNumber)
    {
        ApplyDamage(damage, killerActorNumber);
    }

    public void TakeDamage(int damage, int killerActorNumber = -1)
=======
    private void RPC_TakeDamage(int damage, int attackerViewID)
    {
        ApplyDamage(damage, attackerViewID);
    }

    public void TakeDamage(int damage, int attackerViewID = -1) //-1 nếu ID không xác định
>>>>>>> Stashed changes:Assets/_Scripts/Network/MPHealthController.cs
    {
        if (killerActorNumber == -1 && PhotonNetwork.LocalPlayer != null)
        {
            killerActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
        }

        //chỉ Master mới xử lý trừ máu
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            //gửi rpc tới master để báo trừ máu
<<<<<<< Updated upstream:Assets/_Scripts/Network/MPHealth.cs
            photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.MasterClient, damage, killerActorNumber);
            return;
        }

        ApplyDamage(damage, killerActorNumber);
    }

    private void ApplyDamage(int damage, int killerActorNumber = -1)
=======
            photonView.RPC(nameof(RPC_TakeDamage), RpcTarget.MasterClient, damage, attackerViewID);
            return;
        }

        ApplyDamage(damage, attackerViewID);
    }

    private void ApplyDamage(int damage, int attackerViewID)
>>>>>>> Stashed changes:Assets/_Scripts/Network/MPHealthController.cs
    {
        if (currentHealth == -1 || currentHealth == 0 || IsInvincible) return;

        int damageToHealth = damage;

        if (currentArmor > 0)
        {
            if (currentArmor >= damage)
            {
                // Giáp chịu hết dame
                currentArmor -= damage;
                damageToHealth = 0;
            }
            else
            {
                // Vỡ giáp, dame dư trừ vào máu
                damageToHealth = damage - currentArmor;
                currentArmor = 0;
            }

            // Cập nhật UI trên máy Master
            OnArmorChanged.Invoke();

            // Đồng bộ lượng Giáp mới cho các máy khác
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC(nameof(RPC_SyncArmor), RpcTarget.Others, currentArmor);
            }
        }

        // Nếu còn sát thương dư thì trừ vào máu
        if (damageToHealth > 0)
        {
            currentHealth -= damageToHealth;
            if (currentHealth < 0) currentHealth = 0;

            OnHealthChanged.Invoke();

            // Đồng bộ Máu cho máy khác
            if (PhotonNetwork.IsConnected)
            {
                photonView.RPC(nameof(RPC_SyncHealth), RpcTarget.Others, currentHealth);
            }

            if (currentHealth == 0 && !hasDied)
            {
                hasDied = true;

                if ((CompareTag("Enemy") || isEnemy) && PhotonNetwork.IsConnected)
                {
                    photonView.RPC(nameof(RPC_RewardKill), RpcTarget.All, killerActorNumber);
                }
                else if ((CompareTag("Enemy") || isEnemy) && !PhotonNetwork.IsConnected)
                {
                    PlayerScoreTracker.LocalInstance?.AddKill(1);
                }

                SoundManager.Instance?.PlaySound(dieSound);
                OnDied.Invoke();
                photonView.RPC(nameof(RPC_HandleDeath), RpcTarget.All);

                if (PhotonNetwork.IsMasterClient && attackerViewID != -1)
                {
                    // Tìm đối tượng tấn công
                    PhotonView attackerView = PhotonView.Find(attackerViewID);

                    if (attackerView != null)
                    {
                        //Nếu tự tử, return
                        if (attackerView.ViewID == photonView.ViewID) return;

                        //Nếu kẻ tấn công là Enemy, return
                        var attackerHealth = attackerView.GetComponent<MPHealthController>();
                        if (attackerHealth != null && attackerHealth.IsEnemy) return;

                        //nếu nạn nhân là đối tượng khác không phải địch, return
                        if (!this.isEnemy) return;

                        // Nếu thỏa mãn tất cả, gọi RPC
                        if (attackerView.Owner != null)
                        {
                            photonView.RPC(nameof(RPC_AwardKill), attackerView.Owner);
                        }
                    }
                }
            }
            else
            {
                SoundManager.Instance?.PlaySound(hurtSound);
                OnDamaged.Invoke();
            }
        }

        else
        {
            SoundManager.Instance?.PlaySound(hurtSound);
            OnDamaged.Invoke();
        }
    }

    [PunRPC]
<<<<<<< Updated upstream:Assets/_Scripts/Network/MPHealth.cs
    private void RPC_RewardKill(int killerActorNumber)
    {
        if (PhotonNetwork.LocalPlayer != null && PhotonNetwork.LocalPlayer.ActorNumber == killerActorNumber)
        {
            Debug.Log($"[Kill Tracker] (Actor {killerActorNumber}) killed this Enemy");
            PlayerScoreTracker.LocalInstance?.AddKill(1);
        }
=======
    private void RPC_AwardKill()
    {
        Debug.Log("+1 Kill");
        PlayfabManager.Instance?.SendKillScore(1);
        AddKillToSession();
    }

    private void AddKillToSession()
    {
        // Lấy số kill hiện tại
        int currentKills = 0;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Kills", out object kills))
        {
            currentKills = (int)kills;
        }

        currentKills++;

        // Cập nhật lại vào Photon
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props.Add("Kills", currentKills);
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
>>>>>>> Stashed changes:Assets/_Scripts/Network/MPHealthController.cs
    }

    [PunRPC]
    private void RPC_HandleDeath()
    {
        if (!hasDied)
        {
            hasDied = true;
            SoundManager.Instance?.PlaySound(dieSound);
            OnDied.Invoke();
        }

        //chỉ hiện game over trên máy của mình
        if (triggersGameOver && (!PhotonNetwork.IsConnected || photonView.IsMine))
        {
            StartCoroutine(GameOverCoroutine(3.5f));
        }
    }

    [PunRPC]
    private void RPC_SyncHealth(int newHealth)
    {

        if (newHealth < currentHealth)
        {
            SoundManager.Instance?.PlaySound(hurtSound);
            OnDamaged.Invoke();
        }
        currentHealth = newHealth;
        OnHealthChanged.Invoke();
    }

    [PunRPC]
    private void RPC_SyncArmor(int newArmor)
    {
        if (newArmor < currentArmor)
        {
            SoundManager.Instance?.PlaySound(hurtSound);
            OnDamaged.Invoke();
        }

        currentArmor = newArmor;
        OnArmorChanged.Invoke();
    }

    public void SetHealth(int amount)
    {
        currentHealth = Mathf.Clamp(amount, 0, maximumHealth);
        OnHealthChanged.Invoke();
    }

    public void AddHealth(int amount)
    {
        if (currentHealth == -1 || currentHealth == maximumHealth) return;
        currentHealth = Mathf.Min(currentHealth + amount, maximumHealth);
        OnHealthChanged.Invoke();

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_SyncHealth), RpcTarget.Others, currentHealth);
        }
    }

    public void AddArmor(int amount)
    {
        if (currentHealth <= 0) return;
        if (currentArmor == maximumArmor) return;

        currentArmor = Mathf.Min(currentArmor + amount, maximumArmor);
        OnArmorChanged.Invoke();

        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_SyncArmor), RpcTarget.Others, currentArmor);
        }
    }

    public void ExplodeVFX()
    {
        //gọi RPC cho tất cả
        photonView.RPC(nameof(RPC_PlayExplosionVFX), RpcTarget.All, transform.position, transform.rotation);

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DestroyRoutine());
        }
    }

    private IEnumerator DestroyRoutine()
    {
        //đợi 1 chút để đảm bảo các máy khác đã nhận RPC và xử lý xong
        yield return new WaitForSeconds(0.2f);

        //dọn dẹp vfx
        PhotonNetwork.RemoveRPCs(photonView);
        PhotonNetwork.Destroy(gameObject);
    }

    [PunRPC]
    private void RPC_PlayExplosionVFX(Vector3 pos, Quaternion rot)
    {
        //tạo vfx ngay
        PlayExplosionVFXLocal(pos, rot);

        var renderer = GetComponent<SpriteRenderer>();
        var collider = GetComponent<Collider2D>();

        renderer.enabled = false;
        collider.enabled = false;
    }

    private void PlayExplosionVFXLocal(Vector3 pos, Quaternion rot)
    {
        if (explodeVFXPrefab == null) return;

        var vfx = Instantiate(explodeVFXPrefab, pos, rot);
        Destroy(vfx.gameObject, 2f);
    }

    private IEnumerator GameOverCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        playerHUD.GameOver();
    }
}