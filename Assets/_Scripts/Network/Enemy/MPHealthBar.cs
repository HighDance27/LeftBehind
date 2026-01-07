using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class MPHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthBarForegroundImage;
    [SerializeField] private Image armorBarForegroundImage;
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Camera camera;

    private PhotonView targetView;
    private MPHealthController healthController;

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
            healthController = target.GetComponent<MPHealthController>();
        }

        if (isEnemy)
        {
            gameObject.SetActive(true);
            if (armorBarForegroundImage != null) armorBarForegroundImage.gameObject.SetActive(false);
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

    private void Start()
    {
        if (healthController != null)
        {
            // Tự động đăng ký sự kiện
            healthController.OnHealthChanged.AddListener(() => UpdateHealthBar(healthController));
            healthController.OnArmorChanged.AddListener(() => UpdateArmorBar(healthController));

            //Cập nhật UI ngay lập tức khi game bắt đầu
            UpdateHealthBar(healthController);
            UpdateArmorBar(healthController);
        }
    }

    private void OnDestroy()
    {
        if (healthController != null)
        {
            healthController.OnHealthChanged.RemoveAllListeners();
            healthController.OnArmorChanged.RemoveAllListeners();
        }
    }

    public void UpdateHealthBar(MPHealthController health)
    {
        healthBarForegroundImage.fillAmount = health.RemainingHealthPercentage;
    }

    public void UpdateArmorBar(MPHealthController health)
    {
        if (GetComponentInParent<MPEnemyMovement>() != null)
        {
            return;
        }

        armorBarForegroundImage.fillAmount = health.RemainingArmorPercentage;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            transform.rotation = camera.transform.rotation;
            transform.position = target.position + offset;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
