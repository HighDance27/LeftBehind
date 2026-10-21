using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class InGameScoreboardUI : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup scoreboardCanvasGroup;
    [SerializeField] private Transform entryContainer;
    [SerializeField] private GameObject entryPrefab; // Prefab dòng gồm 3 Text: Rank, Name, Kills
    [SerializeField] private int maxPlayers = 4;

    private readonly List<GameObject> rowPool = new List<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < maxPlayers; i++)
        {
            GameObject row = Instantiate(entryPrefab, entryContainer);
            row.SetActive(false);
            rowPool.Add(row);
        }

        SetVisible(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UpdateScoreboardUI();
            SetVisible(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            SetVisible(false);
        }
    }

    private void SetVisible(bool isVisible)
    {
        if (scoreboardCanvasGroup == null) return;
        scoreboardCanvasGroup.alpha = isVisible ? 1f : 0f;
        scoreboardCanvasGroup.blocksRaycasts = isVisible;
        scoreboardCanvasGroup.interactable = isVisible;
    }

    private void UpdateScoreboardUI()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null) return;

        var sortedPlayers = PhotonNetwork.CurrentRoom.Players.Values
            .OrderByDescending(p => GetPlayerKills(p))
            .ToList();

        for (int i = 0; i < rowPool.Count; i++)
        {
            if (i < sortedPlayers.Count)
            {
                Player player = sortedPlayers[i];
                int kills = GetPlayerKills(player);

                Text[] texts = rowPool[i].GetComponentsInChildren<Text>();
                if (texts.Length >= 3)
                {
                    texts[0].text = $"{i + 1}";
                    texts[1].text = player.NickName;
                    texts[2].text = kills.ToString();

                    Color textColor = player.IsLocal ? Color.yellow : Color.white;
                    texts[0].color = textColor;
                    texts[1].color = textColor;
                    texts[2].color = textColor;
                }

                rowPool[i].SetActive(true);
            }
            else
            {
                rowPool[i].SetActive(false);
            }
        }
    }

    private int GetPlayerKills(Player player)
    {
        if (player.CustomProperties.TryGetValue(PlayerScoreTracker.KILLS_KEY, out object kills))
        {
            return (int)kills;
        }
        return 0;
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey(PlayerScoreTracker.KILLS_KEY))
        {
            if (scoreboardCanvasGroup != null && scoreboardCanvasGroup.alpha > 0)
            {
                UpdateScoreboardUI();
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateScoreboardUI();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateScoreboardUI();
    }
}