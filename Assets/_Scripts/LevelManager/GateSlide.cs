using UnityEngine;

public class GateSlide : MonoBehaviour
{
    [SerializeField] private GameObject vfx;
    [SerializeField] private Transform target;
    [SerializeField] private float speed;
    [SerializeField] private float yOffset;

    private void Update()
    {
        if (UIManager.IsActCleared)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

            //kiểm tra đã tới đích chưa
            bool reachedTarget = Vector3.Distance(transform.position, target.position) < 0.01f;

            if (!reachedTarget)
            {
                Vector3 spawnPos1 = new Vector3(transform.position.x, transform.position.y - yOffset, transform.position.z);
                Vector3 spawnPos2 = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
                Instantiate(vfx, spawnPos1, transform.rotation);
                Instantiate(vfx, spawnPos2, transform.rotation);
            }
        }
    }
}
