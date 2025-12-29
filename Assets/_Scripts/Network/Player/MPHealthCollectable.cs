using UnityEngine;

public class MPHealthCollectable : MonoBehaviour, ICollectableBehaviour
{
    [SerializeField] private int healthAmount;
    public void OnCollected(GameObject player)
    {
        player.GetComponent<MPHealthController>().AddHealth(healthAmount);

    }
}
