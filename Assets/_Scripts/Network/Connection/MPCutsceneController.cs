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
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }

    public override void OnLeftRoom()
    {
        // Bây giờ mới load scene Lobby cục bộ trên máy người chơi
        SceneManager.LoadScene("Lobby");
    }
}
