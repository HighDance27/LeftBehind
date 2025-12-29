using Photon.Pun;
using UnityEngine;

public class MPSceneEnemyManager : MonoBehaviour
{
    public int levelNumber;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (MPEnemyManager.Instance == null)
        {
            Debug.LogError("[SceneManager] MPEnemyManager.Instance is null. Cannot calculate total enemies.");
            return;
        }

        //Tính tổng số Enemy
        int maxTotalEnemies = 0;

        // Tìm tất cả Zone trong Scene
        var activators = FindObjectsOfType<MPEnemyZoneActivator>();

        foreach (var activator in activators)
        {
            maxTotalEnemies += activator.enemiesInZone.Length;
        }

        //Đăng ký tổng số này lên MPEnemyManager và đồng bộ
        MPEnemyManager.Instance.SetMaxPossibleEnemies(maxTotalEnemies);
    }
}
