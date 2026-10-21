using UnityEngine;
using UnityEngine.UI;

public class LeaderboardItem : MonoBehaviour
{
    [SerializeField] private Text rankText;
    [SerializeField] private Text nameText;
    [SerializeField] private Text scoreText;

    public void SetLeaderboardInfo(int rank, string name, int score)
    {
        rankText.text = (rank + 1).ToString(); // Rank bắt đầu từ 0 nên +1
        nameText.text = name;
        scoreText.text = score.ToString();
    }
}