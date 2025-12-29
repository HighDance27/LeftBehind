using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class MPHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarForegroundImage;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Camera camera;

    private PhotonView targetView;

    private void Awake()
    {

        if (camera == null)
        {
            camera = Camera.main;
        }

        //check for parent's layer
        Transform root = transform.parent;
        bool isEnemy = false;

        if (root != null)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            if (root.gameObject.layer == enemyLayer)
            {
                isEnemy = true;
            }
        }

        if (target != null)
        {
            targetView = target.GetComponent<PhotonView>();
        }

        if (isEnemy)
        {
            gameObject.SetActive(true);
            return;
        }

        //nếu gán vào Player đang chơi, ẩn nó
        if (PhotonNetwork.IsConnected && targetView != null && targetView.IsMine)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public void UpdateHealthBar(MPHealthController health)
    {
        healthBarForegroundImage.fillAmount = health.RemainingHealthPercentage;
    }

    private void LateUpdate()
    {
        //giữ không xoay
        transform.rotation = camera.transform.rotation;
        transform.position = target.position + offset;
    }
}
