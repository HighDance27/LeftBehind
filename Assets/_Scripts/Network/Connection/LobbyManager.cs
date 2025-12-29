using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public Text statusText;

    public InputField roomInputField;

    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject editPanel;

    public Text roomName;
    public Text hostText;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObject; //store the objects in scroll view

    public float timeBetweenUpdates = 1.5f;
    private float nextUpdateTime;

    public List<PlayerItem> playerItemsList = new List<PlayerItem>();
    public PlayerItem playerItemPrefab;
    public Transform playerItemParent;

    public GameObject playButton;

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Start()
    {
        string currentName = PlayerPrefs.GetString("SavedUsername", "Player");
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = currentName;
        }
        // If back from game scene
        if (PhotonNetwork.InRoom)
        {
            lobbyPanel.SetActive(false);
            roomPanel.SetActive(true);
            editPanel.SetActive(false);
            roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
            UpdatePlayerList();
        }
        else
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.JoinLobby();
            }
        }
        if (Application.isEditor)
        {
            PhotonNetwork.NickName += "_" + Random.Range(0, 1000);
        }
    }

    private void Update()
    {
        //nếu là Master, hiện nút Play
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount >= 1)
        {
            playButton.SetActive(true);
        }
        else
        {
            playButton.SetActive(false);
        }
    }

    // public void OnClickPlayButton()
    // {
    //     LockRoom();
    //     PhotonNetwork.LoadLevel("MPCutscene1");
    // }

    public void OnClickCreate()
    {
        if (roomInputField.text.Length >= 1)
        {
            statusText.text = "Creating Room...";
            PhotonNetwork.CreateRoom(roomInputField.text, new RoomOptions() { MaxPlayers = 4, BroadcastPropsChangeToAll = true });
        }
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (returnCode == ErrorCode.GameIdAlreadyExists)
        {
            statusText.text = "Room name already exists or started!";
        }
        else
        {
            statusText.text = "Error: " + message;
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom() //gọi tự động khi vừa tham gia phòng
    {
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);
        editPanel.SetActive(false);
        roomName.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name;
        statusText.text = "";
        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }

    }

    private void UpdateRoomList(List<RoomInfo> list)
    {
        //xóa giá trị hiện tại trong list, sau đó xóa list
        foreach (RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        //tạo RoomItem mới, cập nhật tên, sau đó thêm vào list
        foreach (RoomInfo room in list)
        {
            if (!room.IsOpen || !room.IsVisible || room.PlayerCount <= 0)
                continue;

            RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom);
        }
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
        editPanel.SetActive(true);
        statusText.text = "Create Or Join Room";
    }

    public void OnClickExit()
    {
        if (PlayfabManager.Instance != null)
        {
            PlayfabManager.Instance.StopHeartbeat();
        }

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "LastActive", "0" } }
        };
        PlayFabClientAPI.UpdateUserData(request,
        result =>
        {
            Debug.Log("PlayFab Offline Success.");
            PhotonNetwork.Disconnect();
        },
        error =>
        {
            Debug.LogError("PlayFab Offline Failed: " + error.ErrorMessage);
            PhotonNetwork.Disconnect();
        });
    }

    public override void OnDisconnected(Photon.Realtime.DisconnectCause cause)
    {
        SceneManager.LoadScene("_MainMenu");
    }

    public void LockRoom()
    {
        //ngăn người chơi mới vào phòng
        PhotonNetwork.CurrentRoom.IsOpen = false;

        //ẩn phòng khỏi danh sách
        PhotonNetwork.CurrentRoom.IsVisible = false;

        Debug.Log("Room has been hidden and locked.");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    private void UpdatePlayerList()
    {
        //Dọn list cũ khi có Player tham gia hoặc rời
        foreach (PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        //lấy thông tin Player trong phòng dưới dạng key value
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, playerItemParent);
            newPlayerItem.SetPlayerInfo(player.Value);

            if (player.Value == PhotonNetwork.LocalPlayer) //if me
            {
                newPlayerItem.ApplyLocalChanges();
            }
            playerItemsList.Add(newPlayerItem);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer == newMasterClient)
        {
            StartCoroutine(HostTextCouroutine());
            playButton.SetActive(true);
        }

        UpdatePlayerList();
    }

    private IEnumerator HostTextCouroutine()
    {
        hostText.text = "You are the Host now!";
        yield return new WaitForSeconds(3f);
        hostText.text = "";
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public void OnClickEdit()
    {
        SceneManager.LoadScene("AccountManagement");
    }

}
