using System.Collections;
using UnityEngine;

public class MPInvincibility : MonoBehaviour
{
    private MPHealthController healthController;
    private MPSpriteFlash spriteFlash;

    private void Awake()
    {
        healthController = GetComponent<MPHealthController>();
        spriteFlash = GetComponent<MPSpriteFlash>();
    }
    public void StartInvincibility(float invincibilityDuration, Color flashColor, int numberOfFlashes)
    {
        StartCoroutine(InvincibilityCoroutine(invincibilityDuration, flashColor, numberOfFlashes));
    }

    private IEnumerator InvincibilityCoroutine(float invincibilityDuration, Color flashColor, int numberOfFlashes)
    {
        healthController.IsInvincible = true;
        spriteFlash.StartFlash(invincibilityDuration, flashColor, numberOfFlashes);
        yield return new WaitForSeconds(invincibilityDuration);
        healthController.IsInvincible = false;
    }
}
