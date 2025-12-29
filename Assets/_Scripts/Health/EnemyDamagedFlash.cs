using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDamagedFlash : MonoBehaviour
{
    [SerializeField] private float flashDuration;
    [SerializeField] private Color flashColor;
    [SerializeField] private int numberOfFlashes;
    private SpriteFlash spriteFlash;

    private void Awake()
    {
        spriteFlash = GetComponent<SpriteFlash>();
    }

    public void StartFlash()
    {
        spriteFlash.StartFlash(flashDuration, flashColor, numberOfFlashes);
    }
}