using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerScoreTracker : MonoBehaviour
{
    public static PlayerScoreTracker LocalInstance { get; private set; }
    public const string KILLS_KEY = "PlayerKills";

    private PhotonView pv;

    private void Awake()
    {
        pv = GetComponent<PhotonView>();

        if (pv != null && pv.IsMine)
        {
            LocalInstance = this;

            SetKills(0);
        }
    }

    private void Start()
    {
        if (pv != null && pv.IsMine)
        {
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.GetMyCurrentKills(serverKills =>
                {
                    Debug.Log($"[ScoreTracker] Điểm nạp từ PlayFab cho {PhotonNetwork.NickName}: {serverKills}");
                    SetKills(serverKills);
                });
            }
        }
    }

    public void AddKill(int count = 1)
    {
        if (pv == null || !pv.IsMine) return;

        int currentKills = GetCurrentKills();
        int newKills = currentKills + count;

        SetKills(newKills);

        LeaderboardManager.Instance?.AddKill(count);
    }

    private void SetKills(int kills)
    {
        Hashtable props = new Hashtable
        {
            { KILLS_KEY, kills }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public int GetCurrentKills()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(KILLS_KEY, out object kills))
        {
            return (int)kills;
        }
        return 0;
    }
}