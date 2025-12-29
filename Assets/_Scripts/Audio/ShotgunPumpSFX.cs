using System.Collections;
using TopDown.Audio;
using UnityEngine;

public class ShotgunPumpSFX : MonoBehaviour
{
    [SerializeField] private AudioClip pumpClip;
    [SerializeField] private float pumpDelay = 0.1f;
    private Coroutine pumpRoutine;
    public bool isPumping;

    public void TriggerPump()
    {
        if (pumpRoutine != null) StopCoroutine(pumpRoutine);
        pumpRoutine = StartCoroutine(PlayPumpAfterDelay());
    }

    private IEnumerator PlayPumpAfterDelay()
    {
        isPumping = true;
        yield return new WaitForSeconds(pumpDelay);
        SoundManager.Instance.PlaySound(pumpClip);
        isPumping = false;
        pumpRoutine = null;
    }
}
