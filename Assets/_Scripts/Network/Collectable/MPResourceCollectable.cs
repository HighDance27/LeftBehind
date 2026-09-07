using UnityEngine;

public class MPResourceCollectable : MonoBehaviour, ICollectableBehaviour
{
    [SerializeField] private CollectableType type;

    [Header("Settings")]
    [SerializeField] private int healthAmount;
    [SerializeField] private int armorAmount;
    [SerializeField] private int ammoAmount;

    public void OnCollected(GameObject player)
    {
        switch (type)
        {
            case CollectableType.Health:
                RestoreHealth(player);
                break;

            case CollectableType.Armor:
                RestoreArmor(player);
                break;

            case CollectableType.Ammo:
                RestoreAmmo(player);
                break;
        }
    }

    private void RestoreHealth(GameObject player)
    {
        var healthController = player.GetComponent<MPHealthController>();
        if (healthController != null)
        {
            healthController.AddHealth(healthAmount);
        }
    }

    private void RestoreArmor(GameObject player)
    {
        var healthController = player.GetComponent<MPHealthController>();
        if (healthController != null)
        {
            healthController.AddArmor(armorAmount);
        }
    }

    private void RestoreAmmo(GameObject player)
    {
        var weaponSystem = player.GetComponentInChildren<MPWeaponSystem>(true);

        if (weaponSystem == null || weaponSystem.weapons == null)
            return;

        foreach (var weapon in weaponSystem.weapons)
        {
            if (weapon != null)
                weapon.AddAmmo(ammoAmount);
        }
    }
}