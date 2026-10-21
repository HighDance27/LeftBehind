using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab.ProgressionModels;

public class LeaderboardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private GameObject entryPrefab;
    [SerializeField] private Text statusText;

    [SerializeField] private float refreshCooldown = 10f;
    private float lastFetchTime = -999f;
    private bool isFetching = false;

    private void Start()
    {
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);
        RefreshLeaderboard();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleLeaderboard(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            ToggleLeaderboard(false);
        }
    }

    private void ToggleLeaderboard(bool show)
    {
        if (leaderboardPanel == null) return;

        leaderboardPanel.SetActive(show);

        if (show && Time.time - lastFetchTime > refreshCooldown)
        {
            RefreshLeaderboard();
        }
    }

    public void RefreshLeaderboard()
    {
        if (isFetching || LeaderboardManager.Instance == null) return;

        isFetching = true;

        if (statusText != null && entryContainer.childCount == 0)
        {
            statusText.text = "Loading...";
        }

        LeaderboardManager.Instance.GetLeaderboard(
            rankings =>
            {
                isFetching = false;
                lastFetchTime = Time.time;

                if (statusText != null) statusText.text = "";

                foreach (Transform child in entryContainer)
                {
                    Destroy(child.gameObject);
                }

                foreach (var entry in rankings)
                {
                    GameObject row = Instantiate(entryPrefab, entryContainer);

                    Text[] texts = row.GetComponentsInChildren<Text>();
                    if (texts.Length >= 3)
                    {
                        texts[0].text = $"{entry.Rank}";

                        string playerName = !string.IsNullOrEmpty(entry.DisplayName)
                            ? entry.DisplayName
                            : (entry.Entity != null ? entry.Entity.Id : "Unknown");
                        texts[1].text = playerName;

                        texts[2].text = (entry.Scores != null && entry.Scores.Count > 0) ? entry.Scores[0] : "0";
                    }
                }
            },
            error =>
            {
                isFetching = false;
                if (statusText != null && entryContainer.childCount == 0)
                {
                    statusText.text = "Failed to load!";
                }
            }
        );
    }

    public void OnClickClose()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
        }
    }
}