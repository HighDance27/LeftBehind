using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthUI : MonoBehaviour
{
    [SerializeField] private Image healthBarForegroundImage;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Camera camera;

    public void UpdateHealthBar(HealthController healthController)
    {
        healthBarForegroundImage.fillAmount = healthController.RemainingHealthPercentage;
    }

    private void LateUpdate()
    {
        // Keep rotation fixed (no rotation)
        transform.rotation = camera.transform.rotation;
        transform.position = target.position + offset;
    }
}
