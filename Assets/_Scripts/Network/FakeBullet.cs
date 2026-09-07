using UnityEngine;

public class FakeBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 0.5f;
    private float timer;
    private GameObject owner;

    private void OnEnable()
    {
        timer = 0f; // Reset thời gian đếm ngược mỗi khi được lấy ra từ Pool
    }

    public void SetOwner(GameObject shooter)
    {
        owner = shooter;
    }

    void Update()
    {
        transform.position += -transform.up * speed * Time.deltaTime;

        timer += Time.deltaTime;
        if (timer >= lifetime)
            gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (owner != null)
        {
            if (col.gameObject == owner || col.transform.IsChildOf(owner.transform))
            {
                return;
            }
        }

        if (col.CompareTag("Wall") || col.CompareTag("Destroyable")
        || col.CompareTag("Player") || col.CompareTag("Enemy"))
        {
            gameObject.SetActive(false);
        }
    }

}

