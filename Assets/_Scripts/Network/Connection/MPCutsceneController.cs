using Photon.Pun;
using Photon.Realtime;
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
        // Luôn lắng nghe sự kiện nút bấm ngay từ đầu
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnClickNext);
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (nextButton != null) nextButton.gameObject.SetActive(true);
            if (waitForHostText != null) waitForHostText.SetActive(false);
        }
        else
        {
            if (nextButton != null) nextButton.gameObject.SetActive(false);
            if (waitForHostText != null) waitForHostText.SetActive(true);
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
        //load scene Lobby cục bộ trên máy người chơi
        SceneManager.LoadScene("Lobby");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        UpdateUI();
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        Debug.LogWarning("Cutscene Disconnect: " + cause);

        if (PlayfabManager.Instance != null && PlayfabManager.Instance.IsReconnecting)
        {
            return;
        }

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
