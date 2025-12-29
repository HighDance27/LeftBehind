using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace TopDown.UI
{
    public class MPEnemyCounter : MonoBehaviour
    {
        private readonly CompositeDisposable _subs = new CompositeDisposable();

        [Header("UI")]
        [SerializeField] private Text enemyCounterText;

        [Header("UI Backends (optional)")]
        [SerializeField] private PlayerHUD playerHUD;

        private MPEnemyManager _em;
        private int _alive;
        private int _total;
        private int _maxPossible;
        private bool _actClearedShown;

        private void Awake()
        {
            if (playerHUD == null) playerHUD = GetComponentInParent<PlayerHUD>(true);

            //dùng instance của scene ngay
            _em = MPEnemyManager.Instance;
            if (_em == null)
            {
                Debug.LogWarning("[MPEnemyCounter] EnemyManager.Instance is null. Counter will be inactive.");
            }
        }

        private void OnEnable()
        {
            if (_em == null) return;

            //Theo dõi số lượng kẻ địch đang sống
            _em.AliveEnemies
                .ObserveEveryValueChanged(p => p.Value) //Lắng nghe mỗi khi Value thay đổi
                .Subscribe(v =>
                {
                    _alive = v;
                    UpdateLabel();
                    TryShowActCleared(); // Kiểm tra điều kiện thắng (nếu số sống sót bằng 0)
                })
                .AddTo(_subs);

            //Theo dõi tổng số kẻ địch ĐÃ SINH RA
            _em.TotalEnemies
                .ObserveEveryValueChanged(p => p.Value)
                .Subscribe(v =>
                {
                    _total = v;
                    UpdateLabel(); //Cập nhật tổng số địch đã xuất hiện cho đến hiện tại
                })
                .AddTo(_subs);

            //Theo dõi tổng số kẻ địch TỐI ĐA quy định cho màn này
            _em.MaxPossibleEnemies
                .ObserveEveryValueChanged(p => p.Value)
                .Subscribe(v =>
                {
                    _maxPossible = v;
                    TryShowActCleared(); //Kiểm tra xem đã sinh đủ và giết hết chưa để hoàn thành màn
                })
                .AddTo(_subs);
        }

        private void OnDisable()
        {
            _subs.Clear();
        }

        //gọi hàm này khi bắt đầu màn mới để hiển thị lại panel
        public void ResetActFlag() => _actClearedShown = false;

        private void UpdateLabel()
        {
            // Số lượng kẻ thù đã bị tiêu diệt
            int enemiesKilled = _total - _alive;
            // Số lượng kẻ thù CÒN LẠI cần phải tiêu diệt
            int enemiesRemainingToKill = _maxPossible - enemiesKilled;
            enemiesRemainingToKill = Mathf.Max(0, enemiesRemainingToKill);

            if (enemyCounterText)
                enemyCounterText.text = $"Enemies: {enemiesRemainingToKill}/{_maxPossible}";
        }

        private void TryShowActCleared()
        {
            if (_actClearedShown) return;
            if (_maxPossible <= 0) return;
            if (_alive > 0) return;
            if (_total < _maxPossible) return;

            _actClearedShown = true;

            if (playerHUD != null)
            {
                playerHUD.MissionClear();
                return;
            }

            Debug.LogWarning("[MPEnemyCounter] No PlayerHUD/UIManager available to show Act Cleared.");
        }
    }
}
