using UnityEngine;
using Photon.Pun;

public class MPEnemyZoneActivator : MonoBehaviourPun
{
    [Header("Enemies in this zone")]
    public GameObject[] enemiesInZone;

    private bool _activated;

    private void Start()
    {
        foreach (var enemy in enemiesInZone)
        {
            if (enemy != null)
                enemy.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_activated) return;
        if (!other.CompareTag("Player")) return;
        if (!PhotonNetwork.IsMasterClient) return;

        _activated = true;

        photonView.RPC(nameof(RPC_ActivateEnemies), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_ActivateEnemies()
    {
        foreach (var enemy in enemiesInZone)
        {
            if (enemy != null)
                enemy.SetActive(true);
        }

        if (PhotonNetwork.IsMasterClient && MPEnemyManager.Instance != null)
        {
            MPEnemyManager.Instance.RegisterEnemySpawn(enemiesInZone.Length);
        }
    }
}
