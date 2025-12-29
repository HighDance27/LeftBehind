using UnityEngine;

public class MPPlayerInvincible : MonoBehaviour
{
    private MPInvincibility invincibilityController;
    [SerializeField] private float invincibilityDuration;
    [SerializeField] private Color flashColor;
    [SerializeField] private int numberOfFlashes;
    private void Awake()
    {
        invincibilityController = GetComponent<MPInvincibility>();
    }

    public void StartInvincibility()
    {
        invincibilityController.StartInvincibility(invincibilityDuration, flashColor, numberOfFlashes);
    }
}
