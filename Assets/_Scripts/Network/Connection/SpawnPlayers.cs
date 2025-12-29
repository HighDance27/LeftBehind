using System.Collections.Generic;
using Photon.Pun;
using TopDown.CameraControl;
using UnityEngine;

public class SpawnPlayers : MonoBehaviour
{
    public GameObject[] playerPrefabs;
    public CameraController cameraController;

    public float minX;
    public float minY;
    public float maxX;
    public float maxY;

    //dùng để thêm Player vào list khi spawn
    public static List<MPHealthController> ActivePlayers = new List<MPHealthController>();

    private void Awake()
    {
        // Reset danh sách mỗi khi load lại màn chơi để tránh lưu dữ liệu cũ
        ActivePlayers.Clear();
    }

    private void Start()
    {
        Vector2 randomPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));

        //tìm chỉ số của nv đã chọn, gắn mẫu tương ứng trong mảng
        GameObject playerToSpawn = playerPrefabs[(int)PhotonNetwork.LocalPlayer.CustomProperties["playerAvatar"]];
        GameObject player = PhotonNetwork.Instantiate(playerToSpawn.name, randomPosition, Quaternion.identity);

        PhotonView pv = player.GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            if (cameraController != null)
            {
                cameraController.SetTarget(player.transform);
            }
            else
            {
                Debug.LogError("Lỗi: Bạn chưa kéo thả Camera vào SpawnPlayers trên Inspector!");
            }

            // Tìm UI Health Bar trong Scene
            MPHealthUI healthUI = FindObjectOfType<MPHealthUI>();
            if (healthUI != null)
            {
                // Lấy máu của nhân vật vừa sinh ra và đưa cho UI
                healthUI.Initialize(player.GetComponent<MPHealthController>());
            }
        }
    }
}
