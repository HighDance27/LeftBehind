using TopDown.Audio;
using TopDown.Movement;
using UnityEngine;
using Photon.Pun;

public class MPCollectables : MonoBehaviourPun
{
    private ICollectableBehaviour collectableBehaviour;
    [SerializeField] private AudioClip collectSound;

    private void Awake()
    {
        collectableBehaviour = GetComponent<ICollectableBehaviour>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var player = collision.GetComponent<MultiMovement>();
        if (player != null)
        {
            PhotonView pv = player.GetComponent<PhotonView>();
            if (pv != null && pv.IsMine)
            {
                if (collectableBehaviour != null)
                {
                    collectableBehaviour.OnCollected(player.gameObject);
                }
                SoundManager.Instance?.PlaySound(collectSound);

                if (photonView != null)
                {
                    photonView.RPC(nameof(RPC_DestroyObject), RpcTarget.All);
                }
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    [PunRPC]
    private void RPC_DestroyObject()
    {
        Destroy(gameObject);
    }
}
