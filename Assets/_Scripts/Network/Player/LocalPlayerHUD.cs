using Photon.Pun;
using UnityEngine;

public class LocalPlayerHUD : MonoBehaviourPun
{
    [Header("Assign a Canvas prefab that has PlayerHUD component")]
    public PlayerHUD hudPrefab;

    //dùng để lưu trữ instance duy nhất của HUD.
    // Giúp đảm bảo trên máy của một người chơi chỉ có duy nhất 1 HUD được tạo ra.
    private static PlayerHUD _hudInstance;

    private void OnEnable()
    {
        if (!photonView.IsMine) return;

        //tạo hud cho mỗi local Player
        if (_hudInstance == null)
        {
            if (hudPrefab == null)
            {
                Debug.LogError("[LocalPlayerHUDBootstrap] HUD Prefab is not assigned!");
                return;
            }

            _hudInstance = Instantiate(hudPrefab);
            _hudInstance.name = "PlayerHUD (Local)";
            Debug.Log("[LocalPlayerHUDBootstrap] Spawned local PlayerHUD");
        }
        var ws = GetComponentInChildren<MPWeaponSystem>(true);
        if (ws == null)
        {
            Debug.LogWarning("[LocalPlayerHUDBootstrap] No WeaponSystem found in local player prefab!");
            return;
        }

        // kết nối WeaponSystem vào HUD + MPAmmoCounter
        _hudInstance.SetWeaponSystem(ws);
    }
}
