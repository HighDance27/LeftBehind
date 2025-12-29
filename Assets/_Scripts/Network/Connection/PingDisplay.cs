using Photon.Pun;
using TMPro;
using UnityEngine;

public class PingDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pingText;
    [SerializeField] private float updateInterval = 0.5f; // Cập nhật mỗi 0.5s để đỡ giật số

    private float timer;

    void Update()
    {
        // Chỉ chạy khi đã kết nối
        if (PhotonNetwork.IsConnected)
        {
            timer += Time.deltaTime;
            if (timer >= updateInterval)
            {
                ShowPing();
                timer = 0f;
            }
        }
    }

    void ShowPing()
    {
        int ping = PhotonNetwork.GetPing();

        pingText.text = $"Ping: {ping} ms";

        // Đổi màu chữ theo chất lượng mạng (Tùy chọn)
        if (ping < 100)
            pingText.color = Color.green;  // Mạng tốt
        else if (ping < 200)
            pingText.color = Color.yellow; // Trung bình
        else
            pingText.color = Color.red;    // Mạng lag
    }
}
