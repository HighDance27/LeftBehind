using UnityEngine;
using UnityEngine.UI;

public class MPHealthUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MPHealthController healthController;
    [SerializeField] private Image healthBarForegroundImage;
    [SerializeField] private Image armorBarForegroundImage;

    public void Initialize(MPHealthController playerHealth)
    {
        Debug.Log("MPHealthUI: Đang khởi tạo kết nối với Player..."); // [DEBUG 1]

        // 1. Gỡ Listener cũ (nếu có) để tránh lỗi
        if (healthController != null)
        {
            healthController.OnHealthChanged.RemoveListener(UpdateHealthBar);
            healthController.OnArmorChanged.RemoveListener(UpdateArmorBar);
        }

        // 2. Gán Player mới
        healthController = playerHealth;

        if (healthController != null)
        {
            // Đăng ký sự kiện
            healthController.OnHealthChanged.AddListener(UpdateHealthBar);
            healthController.OnArmorChanged.AddListener(UpdateArmorBar);

            // Cập nhật giao diện ngay lập tức
            UpdateHealthBar();
            UpdateArmorBar();

            Debug.Log("MPHealthUI: Kết nối thành công!"); // [DEBUG 2]
        }
        else
        {
            Debug.LogError("MPHealthUI: Player Health Controller bị NULL!"); // [DEBUG ERROR]
        }
    }

    private void OnDisable()
    {
        // 3. Sửa lỗi thiếu dấu ngoặc nhọn {} ở phiên bản cũ
        if (healthController != null)
        {
            healthController.OnHealthChanged.RemoveListener(UpdateHealthBar);
            healthController.OnArmorChanged.RemoveListener(UpdateArmorBar);
        }
    }

    public void UpdateHealthBar()
    {
        if (healthController == null) return;

        // Debug kiểm tra chỉ số máu thực tế
        // Debug.Log($"UI Máu: {healthController.CurrentHealth}/{healthController.MaximumHealth}");

        if (healthBarForegroundImage != null)
        {
            healthBarForegroundImage.fillAmount = healthController.RemainingHealthPercentage;
        }
        else
        {
            Debug.LogWarning("MPHealthUI: Chưa gắn ảnh HealthBarForegroundImage trong Inspector!");
        }
    }

    public void UpdateArmorBar()
    {
        if (healthController == null) return;

        if (armorBarForegroundImage != null)
        {
            armorBarForegroundImage.fillAmount = healthController.RemainingArmorPercentage;
        }
        else
        {
            Debug.LogWarning("MPHealthUI: Chưa gắn ảnh ArmorBarForegroundImage trong Inspector!");
        }
    }
}