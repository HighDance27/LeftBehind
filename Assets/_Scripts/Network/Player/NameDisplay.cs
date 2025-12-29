using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class NameDisplay : MonoBehaviourPun
{
    [SerializeField] private Text nameText;

    private void Start()
    {
        if (photonView.IsMine)
        {
            nameText.text = PhotonNetwork.NickName;
        }
        else
        {
            nameText.text = photonView.Owner.NickName;
        }
    }

    void LateUpdate()
    {
        transform.rotation = Camera.main.transform.rotation;
    }

}