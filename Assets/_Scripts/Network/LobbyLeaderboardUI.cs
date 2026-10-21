using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ProgressionModels;

public class LobbyLeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Text statusText;
    [SerializeField] private Button closeButton;
    private LobbyManager manager;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePanel);
        }

        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }

        manager = FindObjectOfType<LobbyManager>();
    }

    private void Update()
    {
        if (leaderboardPanel != null && leaderboardPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    public void OpenPanel()
    {
        if (leaderboardPanel == null)
        {
            Debug.LogError("[LobbyLeaderboardUI] Chưa kéo LeaderboardPanel vào Inspector!");
            return;
        }
        manager.editPanel.SetActive(false);
        leaderboardPanel.SetActive(true);

        CanvasGroup cg = leaderboardPanel.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }

        FetchLeaderboardData();
    }


    public void ClosePanel()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }

        manager.editPanel.SetActive(true);
    }

    public void FetchLeaderboardData()
    {
        if (statusText != null) statusText.text = "Loading...";

        foreach (Transform child in entryContainer)
        {
            Destroy(child.gameObject);
        }

        if (LeaderboardManager.Instance == null)
        {
            if (statusText != null) statusText.text = "LeaderboardManager not initialized!";
            Debug.LogError("[LobbyLeaderboardUI] LeaderboardManager.Instance is NULL! Hãy chạy game từ Scene đầu tiên.");
            return;
        }

        LeaderboardManager.Instance.GetLeaderboard(
            rankings =>
            {
                if (statusText != null) statusText.text = "";

                if (rankings == null || rankings.Count == 0)
                {
                    if (statusText != null) statusText.text = "No players found!";
                    return;
                }

                foreach (var entry in rankings)
                {
                    GameObject row = Instantiate(entryPrefab, entryContainer);

                    Text[] texts = row.GetComponentsInChildren<Text>();
                    if (texts.Length >= 3)
                    {
                        texts[0].text = $"#{entry.Rank}";

                        string playerName = !string.IsNullOrEmpty(entry.DisplayName)
                            ? entry.DisplayName
                            : (entry.Entity != null ? entry.Entity.Id : "Player");
                        texts[1].text = playerName;

                        texts[2].text = (entry.Scores != null && entry.Scores.Count > 0) ? entry.Scores[0] : "0";
                    }
                }
            },
            error =>
            {
                if (statusText != null) statusText.text = "Failed to load leaderboard!";
                Debug.LogError($"[LobbyLeaderboardUI] Lỗi PlayFab: {error}");
            }
        );
    }
}