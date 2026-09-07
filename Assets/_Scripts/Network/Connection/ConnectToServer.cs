using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Realtime;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public Text buttonText;

    private void Start()
    {
        // Tự động kết nối ngay khi vào Scene
        OnClickConnect();
    }

    public void OnClickConnect()
    {
        string savedDisplayName = PlayerPrefs.GetString("SavedUsername", "Player");
        PhotonNetwork.NickName = savedDisplayName;

        // Gán PlayFabId làm UserId duy nhất để Photon nhận diện và chặn login đè
        AuthenticationValues authValues = new AuthenticationValues();
        authValues.UserId = PlayerPrefs.GetString("PlayFabId");
        PhotonNetwork.AuthValues = authValues;

        buttonText.text = "Connecting...";
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }

    public void ExitToMenu()
    {
        buttonText.text = "Returning...";

        //nếu đang kết nối thì ngắt
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        else
        {
            //load thẳng nếu không kết nối
            SceneManager.LoadScene("_MainMenu");
        }
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Lobby");
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        if (PlayfabManager.Instance != null && PlayfabManager.Instance.IsLoggingOut)
        {
            return;
        }

        Debug.LogWarning("Connect Failed: " + cause);

        if (PlayfabManager.Instance != null)
        {
            PlayfabManager.Instance.StopHeartbeat();
            PlayfabManager.Instance.ForceLogout();
        }
        else
        {
            SceneManager.LoadScene("_MainMenu");
        }
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.LogError("Photon Auth Failed: " + debugMessage);

        if (PlayfabManager.Instance != null)
        {
            // Token PlayFab có vấn đề, Cần đăng nhập lại
            PlayfabManager.Instance.ForceLogout();
        }
    }
}
