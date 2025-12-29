using TopDown.Audio;
using TopDown.Movement;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    private ICollectableBehaviour collectableBehaviour;
    [SerializeField] private AudioClip collectSound;

    private void Awake()
    {
        collectableBehaviour = GetComponent<ICollectableBehaviour>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<Player>();
        if (player != null)
        {
            collectableBehaviour.OnCollected(player.gameObject);
            SoundManager.Instance?.PlaySound(collectSound);
            Destroy(gameObject);
        }
    }
}
