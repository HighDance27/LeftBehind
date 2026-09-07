using UnityEngine;
using UnityEngine.UI;

public class StatUI : MonoBehaviour
{
    [SerializeField] private Image healthBarForegroundImage;
    [SerializeField] private Image armorBarForegroundImage;

    public void UpdateHealthBar(HealthController healthController)
    {
        healthBarForegroundImage.fillAmount = healthController.RemainingHealthPercentage;
    }

    public void UpdateArmorBar(HealthController healthController)
    {
        armorBarForegroundImage.fillAmount = healthController.RemainingArmorPercentage;
    }
}
