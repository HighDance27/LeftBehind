using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using PlayFab;
using System.Collections.Generic;
using PlayFab.ClientModels;

public class LevelSelectManager : MonoBehaviour
{
    public Button[] levelButtons;
    private LobbyManager lobbyManager;

    private void Start()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
        UpdateLevelButtons();
    }

    public void UpdateLevelButtons()
    {
        // Lấy số màn đã mở khóa, mặc định là màn 1
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        Debug.Log("Số Level đã mở khóa hiện tại là: " + unlockedLevel);
        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNum = i + 1;
            // Nếu số màn nhỏ hơn hoặc bằng số đã mở khóa thì cho phép bấm
            if (levelNum <= unlockedLevel)
            {
                levelButtons[i].interactable = true;
            }
            else
            {
                levelButtons[i].interactable = false;
            }
        }
    }

    public void OnClickSelectLevel(int levelNumber)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (lobbyManager != null)
        {
            lobbyManager.LockRoom();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy LobbyManager để khóa phòng!");
        }

        string sceneName = "MPCutscene" + levelNumber;
        PhotonNetwork.LoadLevel(sceneName);
    }

}