using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkGameOver : MonoBehaviourPun
{
    public static NetworkGameOver Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    [PunRPC]
    private void RPC_RestartLevel()
    {
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex);
    }

    public void RequestRestart()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        photonView.RPC(nameof(RPC_RestartLevel), RpcTarget.All);
    }
}
