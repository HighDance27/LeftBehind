using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown.UI
{
    public class EnemyCounter : MonoBehaviour
    {
        private CompositeDisposable subscriptions = new CompositeDisposable();

        [Header("References")]
        [SerializeField] private Text enemyCounterText;
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private UIManager uiManager;

        private int aliveEnemies;
        private int totalEnemies;
        private bool actClearedShown = false;

        private void OnEnable()
        {
            //Theo dõi số lượng Enemy còn sống
            enemyManager.AliveEnemies
                .ObserveEveryValueChanged(p => p.Value) //tự động theo dõi biến Value của enemyManager,
                                                        // để khi giá trị thay đổi, nó sẽ nhảy ra thông báo
                .Subscribe(value => //Khi nhận được thông báo, nó sẽ thực hiện đoạn bên trong
                {
                    aliveEnemies = value;
                    UpdateEnemyCounter(aliveEnemies, totalEnemies); //Update lên UI

                    //Chỉ hiện 1 lần, đảm bảo màn chơi này có Enemy
                    if (!actClearedShown && totalEnemies > 0 && aliveEnemies <= 0)
                    {
                        actClearedShown = true;
                        if (uiManager) uiManager.ShowActCleared(uiManager.actNumber);
                    }
                })
                .AddTo(subscriptions);

            //Theo dõi số lượng Enemy trong hồi này
            enemyManager.TotalEnemies
                .ObserveEveryValueChanged(p => p.Value)
                .Subscribe(value =>
                {
                    totalEnemies = value;
                    UpdateEnemyCounter(aliveEnemies, totalEnemies);
                })
                .AddTo(subscriptions);
        }

        private void OnDisable()
        {
            subscriptions.Clear();
        }

        private void UpdateEnemyCounter(int alive, int total)
        {
            if (enemyCounterText)
                enemyCounterText.text = $"Enemies: {alive}/{total}";
        }
    }
}
