using UnityEngine;

public class PlayerDamagedFlash : MonoBehaviour
{
    [SerializeField] private SpriteFlash spriteFlash;
    [SerializeField] private float flashDuration = 0.2f;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private int numberOfFlashes = 2;

    public void FlashOnDamage()
    {
        spriteFlash.StartFlash(flashDuration, flashColor, numberOfFlashes);
    }
}
