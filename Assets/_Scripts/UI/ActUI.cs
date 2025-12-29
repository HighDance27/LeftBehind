using UnityEngine;
using UnityEngine.UI;

public class ActUI : MonoBehaviour
{
    [Header("Act Clear")]
    [SerializeField] private GameObject actClearedPanel;
    [SerializeField] private Text actClearedLabel;
    [SerializeField] private float actClearedDuration = 2f;

    public static bool IsActCleared { get; private set; }

    public void ShowActCleared(int actNumber)
    {
        IsActCleared = true;
        if (actClearedLabel) actClearedLabel.text = $"Act {actNumber} Cleared!";
        if (actClearedPanel) actClearedPanel.SetActive(true);

        //lấy tên hàm dưới dạng chuỗi
        //hủy chạy hàm của lần trước nếu có
        CancelInvoke(nameof(HideActCleared));
        //chạy hàm ... sau khoảng thời gian ...
        Invoke(nameof(HideActCleared), actClearedDuration);
    }

    public void HideActCleared()
    {
        if (actClearedPanel) actClearedPanel.SetActive(false);
        IsActCleared = false;
    }
}
