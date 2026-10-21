using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class Scoreboard : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text killText;

    public void Initialize(Player player)
    {
        nameText.text = player.NickName;

        // Lấy số kill từ Custom Properties của Photon
        // Nếu chưa có thì mặc định là 0
        if (player.CustomProperties.TryGetValue("Kills", out object kills))
        {
            killText.text = kills.ToString();
        }
        else
        {
            killText.text = "0";
        }
    }
}