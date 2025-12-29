using UnityEngine;

public class FakeBullet : MonoBehaviour
{
    public float speed = 10f;
    public float lifetime = 0.5f;
    private float timer;

    private void OnEnable()
    {
        timer = 0f; // Reset thời gian đếm ngược mỗi khi được lấy ra từ Pool
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
        if (col.CompareTag("Wall") || col.CompareTag("Destroyable")
        || col.CompareTag("Player") || col.CompareTag("Enemy"))
        {
            gameObject.SetActive(false);
        }
    }

}

