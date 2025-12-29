using UnityEngine;
using UnityEngine.UI;

public class MPHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MPHealthController healthController;
    [SerializeField] private Image healthBarForegroundImage;

    public void Initialize(MPHealthController playerHealth)
    {
        //gỡ Listener cũ
        if (healthController != null)
        {
            healthController.OnHealthChanged.RemoveListener(UpdateHealthBar);
        }

        //gán Player mới
        healthController = playerHealth;

        if (healthController != null)
        {
            healthController.OnHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(); // Cập nhật ngay lập tức
        }
    }

    private void OnDisable()
    {
        if (healthController != null)
            healthController.OnHealthChanged.RemoveListener(UpdateHealthBar);
    }

    public void UpdateHealthBar()
    {
        if (healthController == null || healthBarForegroundImage == null)
            return;

        healthBarForegroundImage.fillAmount = healthController.RemainingHealthPercentage;
    }
}
