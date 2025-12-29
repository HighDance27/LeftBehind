using TopDown.Shooting;
using TopDown.UI;
using UnityEngine;

public class AmmoCollectable : MonoBehaviour, ICollectableBehaviour
{
    [SerializeField] private int ammoAmount;

    public void OnCollected(GameObject player)
    {
        var weaponSystem = player.GetComponentInChildren<WeaponSystem>(true);

        if (weaponSystem == null || weaponSystem.weapons == null)
            return;

        //Thêm đạn vào các vũ khí hiện có
        foreach (var weapon in weaponSystem.weapons)
        {
            if (weapon != null)
                weapon.AddAmmo(ammoAmount);
        }
    }
}
