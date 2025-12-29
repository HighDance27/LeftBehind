using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMenu : MonoBehaviourPunCallbacks
{
    private int currentSceneIndex;

    public void LoadMenu()
    {
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        PlayerPrefs.SetInt("SavedScene", currentSceneIndex);
        //nếu đang kết nối mạng, rồi phòng trước mới disconnect
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene(0);
        }
        else
        {
            // Offline mode
            SceneManager.LoadScene(0);
        }
    }
}
