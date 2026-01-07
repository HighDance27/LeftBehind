using UnityEngine;

public enum CollectableType
{
    Health,
    Ammo,
    Armor
}

public class ResourceCollectable : MonoBehaviour, ICollectableBehaviour
{

    [SerializeField] private CollectableType type;
    [SerializeField] private int healthAmount;
    [SerializeField] private int ammoAmount;
    [SerializeField] private int armorAmount;

    public void OnCollected(GameObject player)
    {
        switch (type)
        {
            case CollectableType.Health:
                RestoreHealth(player);
                break;

            case CollectableType.Ammo:
                RestoreAmmo(player);
                break;

            case CollectableType.Armor:
                RestoreArmor(player);
                break;
        }
    }

    private void RestoreHealth(GameObject player)
    {
        var healthController = player.GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.AddHealth(healthAmount);
        }
    }

    private void RestoreArmor(GameObject player)
    {
        var healthController = player.GetComponent<HealthController>();

        if (healthController != null)
        {
            healthController.AddArmor(armorAmount);
        }
    }

    private void RestoreAmmo(GameObject player)
    {
        var weaponSystem = player.GetComponentInChildren<WeaponSystem>(true);
        if (weaponSystem != null && weaponSystem.weapons != null)
        {
            foreach (var weapon in weaponSystem.weapons)
            {
                if (weapon != null)
                    weapon.AddAmmo(ammoAmount);
            }
        }
    }

}