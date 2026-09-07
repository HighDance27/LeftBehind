using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MPCutsceneController : MonoBehaviourPunCallbacks
{
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject waitForHostText;
    [SerializeField] private string sceneName;

    private void Start()
    {
        //chỉ có Master thấy và bấm được nút Play
        if (PhotonNetwork.IsMasterClient)
        {
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(true);
                nextButton.onClick.AddListener(OnClickNext);
            }

            if (waitForHostText != null)
                waitForHostText.SetActive(false);
        }
        else
        {
            if (nextButton != null)
                nextButton.gameObject.SetActive(false);

            if (waitForHostText != null)
                waitForHostText.SetActive(true);
        }
    }

    private void OnClickNext()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        PhotonNetwork.LoadLevel(sceneName);
    }

    public void BackToLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
            else
            {
                SceneManager.LoadScene("Lobby");
            }
        }
        else
        {
            SceneManager.LoadScene("_MainMenu");
        }
    }

    public override void OnLeftRoom()
    {
        // Bây giờ mới load scene Lobby cục bộ trên máy người chơi
        SceneManager.LoadScene("Lobby");
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogWarning("Cutscene Disconnect: " + cause);

        // Nếu PlayFab đang xử lý logout, return
        if (PlayfabManager.Instance != null && PlayfabManager.Instance.IsLoggingOut) return;

        if (PlayfabManager.Instance != null)
        {
            PlayfabManager.Instance.ForceLogout();
        }
        // Mất mạng khi đang xem Cutscene, Về Menu chính
        SceneManager.LoadScene("_MainMenu");
    }
}
