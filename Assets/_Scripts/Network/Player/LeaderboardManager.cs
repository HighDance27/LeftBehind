using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ProgressionModels;

public class LeaderboardManager : MonoBehaviour
{
    public static LeaderboardManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private string leaderboardName = "TotalEnemyKilled";

    public int CurrentKills { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentKills = PlayerPrefs.GetInt("TotalEnemyKilled", 0);
    }

    public void AddKill(int count = 1)
    {
        CurrentKills += count;
        PlayerPrefs.SetInt("TotalEnemyKilled", CurrentKills);
        PlayerPrefs.Save();

        SubmitScoreV2(CurrentKills);
    }

    private void SubmitScoreV2(int score)
    {
        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            Debug.LogWarning("[Leaderboard] Chưa đăng nhập PlayFab!");
            return;
        }

        // Lấy EntityId của người chơi hiện tại
        string entityId = PlayFabSettings.staticPlayer.EntityId;

        if (string.IsNullOrEmpty(entityId))
        {
            Debug.LogError("[Leaderboard] EntityId đang null! Hãy đảm bảo đã hoàn tất Login thành công.");
            return;
        }

        var request = new UpdateLeaderboardEntriesRequest
        {
            LeaderboardName = leaderboardName,
            Entries = new List<LeaderboardEntryUpdate>
            {
                new LeaderboardEntryUpdate
                {
                    EntityId = entityId,
                    Scores = new List<string> { score.ToString() }
                }
            }
        };

        PlayFabProgressionAPI.UpdateLeaderboardEntries(request,
            result => Debug.Log($"[PlayFab v2] Đã cập nhật BXH: +1 Kill (Tổng: {score})"),
            error => Debug.LogError($"[PlayFab v2] Lỗi cập nhật điểm: {error.GenerateErrorReport()}")
        );
    }

    public void GetLeaderboard(Action<List<EntityLeaderboardEntry>> onSuccess, Action<string> onError = null)
    {
        var request = new GetEntityLeaderboardRequest
        {
            LeaderboardName = leaderboardName,
            PageSize = 10,
            StartingPosition = 1
        };

        PlayFabProgressionAPI.GetLeaderboard(request,
            result => onSuccess?.Invoke(result.Rankings),
            error =>
            {
                Debug.LogError($"{error.GenerateErrorReport()}");
                onError?.Invoke(error.GenerateErrorReport());
            }
        );
    }

    public void GetMyCurrentKills(Action<int> onComplete)
    {
        // 1. Đọc điểm lưu tạm từ PlayerPrefs trước
        int cachedKills = PlayerPrefs.GetInt("TotalEnemyKilled", 0);

        if (!PlayFabClientAPI.IsClientLoggedIn())
        {
            onComplete?.Invoke(cachedKills);
            return;
        }

        string entityId = PlayFabSettings.staticPlayer.EntityId;
        string entityType = string.IsNullOrEmpty(PlayFabSettings.staticPlayer.EntityType)
                            ? "title_player_account"
                            : PlayFabSettings.staticPlayer.EntityType;

        var request = new GetLeaderboardAroundEntityRequest
        {
            LeaderboardName = leaderboardName,
            Entity = new EntityKey
            {
                Id = entityId,
                Type = entityType
            }
        };

        PlayFabProgressionAPI.GetLeaderboardAroundEntity(request,
            result =>
            {
                int remoteScore = 0;
                if (result.Rankings != null && result.Rankings.Count > 0)
                {
                    var myEntry = result.Rankings[0];
                    if (myEntry.Scores != null && myEntry.Scores.Count > 0)
                    {
                        int.TryParse(myEntry.Scores[0], out remoteScore);
                    }
                }

                if (remoteScore > cachedKills)
                {
                    cachedKills = remoteScore;
                    PlayerPrefs.SetInt("TotalEnemyKilled", cachedKills);
                    PlayerPrefs.Save();
                }

                CurrentKills = cachedKills;
                onComplete?.Invoke(cachedKills);
            },
            error =>
            {
                Debug.LogWarning($"[PlayFab] Không lấy được điểm: {error.GenerateErrorReport()}, dùng điểm từ máy: {cachedKills}");
                onComplete?.Invoke(cachedKills);
            }
        );
    }
}