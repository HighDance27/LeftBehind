using UniRx;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    //Dùng để tự thông báo cho các bên đăng ký mỗi khi Value thay đổi, dùng để gán dữ liệu trực tiếp vào UI
    public IntReactiveProperty AliveEnemies { get; private set; } = new IntReactiveProperty(0);
    public IntReactiveProperty TotalEnemies { get; private set; } = new IntReactiveProperty(0);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void RegisterEnemy()
    {
        TotalEnemies.Value++;
        AliveEnemies.Value++;
    }

    public void OnEnemyDied()
    {
        AliveEnemies.Value--;
    }
}
