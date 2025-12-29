using UnityEngine;

public class MPAmmoCollectable : MonoBehaviour, ICollectableBehaviour
{
    [SerializeField] private int ammoAmount;
    public void OnCollected(GameObject player)
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
