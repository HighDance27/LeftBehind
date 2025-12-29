using UnityEngine;

public class PlayerDamagedInvincibility : MonoBehaviour
{
    private InvincibilityController invincibilityController;

    [SerializeField] private float invincibilityDuration;
    [SerializeField] private Color flashColor;
    [SerializeField] private int numberOfFlashes;

    private void Awake()
    {
        invincibilityController = GetComponent<InvincibilityController>();
    }

    public void StartInvincibility()
    {
        invincibilityController.StartInvincibility(invincibilityDuration, flashColor, numberOfFlashes);
    }
}
