using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    public Text playerName;
    public GameObject leftArrow;
    public GameObject rightArrow;

    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
    public Image playerAvatar;
    public Sprite[] avatars;

    Player player;

    [Header("Info Panels")]
    public GameObject[] infoPanels; // 4 panel cho 4 avatar
    private bool showingInfo = false;
    private int currentAvatarIndex = 0;

    private void Awake()
    {
        if (leftArrow) leftArrow.SetActive(false);
        if (rightArrow) rightArrow.SetActive(false);
    }

    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
        player = _player;

        bool isLocal = player == PhotonNetwork.LocalPlayer;
        if (leftArrow) leftArrow.SetActive(isLocal);
        if (rightArrow) rightArrow.SetActive(isLocal);

        showingInfo = false;
        if (playerAvatar) playerAvatar.gameObject.SetActive(true);
        if (playerName) playerName.gameObject.SetActive(true);
        UpdateInfoPanelsVisibility();

        UpdatePlayerItem(player);
    }

    public void ApplyLocalChanges()
    {
        if (player == PhotonNetwork.LocalPlayer)
        {
            if (leftArrow) leftArrow.SetActive(true);
            if (rightArrow) rightArrow.SetActive(true);
        }
    }

    public void OnClickLeftArrow()
    {
        if (player != PhotonNetwork.LocalPlayer)
            return;

        if ((int)playerProperties["playerAvatar"] == 0) //Hastable trả về kiểu object nên phải ép kiểu
        {
            playerProperties["playerAvatar"] = avatars.Length - 1;
        }
        else
        {
            playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] - 1;
        }
        //thông báo cho các players biết custom property vừa thay đổi cho 1 Player
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void OnClickRightArrow()
    {
        if (player != PhotonNetwork.LocalPlayer)
            return;

        if ((int)playerProperties["playerAvatar"] == avatars.Length - 1)
        {
            playerProperties["playerAvatar"] = 0;
        }
        else
        {
            playerProperties["playerAvatar"] = (int)playerProperties["playerAvatar"] + 1;
        }
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    public void OnClickAvatar()
    {
        if (player != PhotonNetwork.LocalPlayer)
            return;

        showingInfo = !showingInfo;

        if (playerAvatar)
            playerAvatar.gameObject.SetActive(!showingInfo);
        if (playerName)
            playerName.gameObject.SetActive(!showingInfo);
        if (rightArrow)
            rightArrow.gameObject.SetActive(!showingInfo);
        if (leftArrow)
            leftArrow.gameObject.SetActive(!showingInfo);

        UpdateInfoPanelsVisibility();
    }

    private void UpdateInfoPanelsVisibility()
    {
        if (infoPanels == null) return;

        for (int i = 0; i < infoPanels.Length; i++)
        {
            if (infoPanels[i] == null) continue;

            if (showingInfo && i == currentAvatarIndex)
            {
                infoPanels[i].SetActive(true);
            }
            else
            {
                infoPanels[i].SetActive(false);
            }
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //nếu có Player thay đổi property, hàm sẽ chạy trên tất cả client
        if (player == targetPlayer)
        {
            UpdatePlayerItem(targetPlayer);
        }
    }

    private void UpdatePlayerItem(Player player)
    {
        if (player.CustomProperties.ContainsKey("playerAvatar"))
        {
            currentAvatarIndex = (int)player.CustomProperties["playerAvatar"];
            currentAvatarIndex = Mathf.Clamp(currentAvatarIndex, 0, avatars.Length - 1); //limit index
            playerAvatar.sprite = avatars[currentAvatarIndex];
            playerProperties["playerAvatar"] = currentAvatarIndex;
        }
        else
        {
            currentAvatarIndex = 0;
            playerProperties["playerAvatar"] = 0;
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);
        }
        UpdateInfoPanelsVisibility();
    }
}
