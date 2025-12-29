using UniRx;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class MPEnemyManager : MonoBehaviourPunCallbacks
{
    public static MPEnemyManager Instance;

    public IntReactiveProperty AliveEnemies { get; private set; } = new IntReactiveProperty(0);
    public IntReactiveProperty TotalEnemies { get; private set; } = new IntReactiveProperty(0);
    public IntReactiveProperty MaxPossibleEnemies { get; private set; } = new IntReactiveProperty(0);

    // Các keys để lưu trữ dữ liệu trong Room của Photon
    private const string ROOM_ALIVE = "aliveEnemies";
    private const string ROOM_TOTAL = "totalEnemies";
    private const string ROOM_MAX_POSSIBLE = "maxEnemies";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
        {
            AliveEnemies.Value = 0;
            TotalEnemies.Value = 0;
            MaxPossibleEnemies.Value = 0;
            PushRoomProps(); //Ghi đè giá trị 0 lên Room để mọi người cùng đồng bộ
        }
    }

    //Dùng để thiết lập tổng số kẻ thù tối đa của scene
    public void SetMaxPossibleEnemies(int maxCount)
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        MaxPossibleEnemies.Value = maxCount;
        PushRoomProps(); // Cập nhật thay đổi lên Photon Room
        Debug.Log($"[EnemyManager] MaxPossibleEnemies set to: {maxCount}");
    }

    // Dùng để tăng số lượng kẻ thù đã được kích hoạt
    public void RegisterEnemySpawn(int count = 1)
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        TotalEnemies.Value += count;
        AliveEnemies.Value += count;
        PushRoomProps(); // Đồng bộ con số mới cho cả phòng
        Debug.Log($"[EnemyManager] SpawnRegistered -> Total={TotalEnemies.Value}, Alive={AliveEnemies.Value}");
    }

    public void OnEnemyDied()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;
        var newAlive = Mathf.Max(0, AliveEnemies.Value - 1); //giảm số lượng địch đang sống và không cho phép âm

        //Chỉ cập nhật Alive. Chúng ta sẽ dùng PushRoomProps() để đồng bộ toàn bộ.
        AliveEnemies.Value = newAlive;

        // Cập nhật thuộc tính các Enemy còn sống lên Photon Room
        var props = new Hashtable { [ROOM_ALIVE] = newAlive };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        Debug.Log($"[EnemyManager] OnEnemyDied -> Alive={AliveEnemies.Value}");
    }

    //Đẩy toàn bộ dữ liệu hiện tại lên Custom Properties của Room
    private void PushRoomProps()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.IsMasterClient) return;

        var props = new Hashtable
        {
            [ROOM_ALIVE] = AliveEnemies.Value,
            [ROOM_TOTAL] = TotalEnemies.Value,
            [ROOM_MAX_POSSIBLE] = MaxPossibleEnemies.Value
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    // Lấy dữ liệu từ Room về máy cục bộ
    private void PullRoomProps()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null) return;

        var roomProps = PhotonNetwork.CurrentRoom.CustomProperties;

        if (roomProps.ContainsKey(ROOM_ALIVE))
        {
            // Truy cập trực tiếp bằng Key, kết quả trả về là kiểu object
            object value = roomProps[ROOM_ALIVE];
            // Ép kiểu sang int và gán vào bên UniRx
            AliveEnemies.Value = (int)value;
        }

        if (roomProps.ContainsKey(ROOM_TOTAL))
        {
            object value = roomProps[ROOM_TOTAL];
            TotalEnemies.Value = (int)value;
        }

        if (roomProps.ContainsKey(ROOM_MAX_POSSIBLE))
        {
            object value = roomProps[ROOM_MAX_POSSIBLE];
            MaxPossibleEnemies.Value = (int)value;
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable changedProps)
    {
        if (changedProps.ContainsKey(ROOM_ALIVE))
        {
            object valueAlive = changedProps[ROOM_ALIVE]; // Lấy giá trị ra dưới dạng object
            int aliveCount = (int)valueAlive;            //Ép kiểu object sang int
            AliveEnemies.Value = aliveCount;             // Gán vào biến của UniRx
        }

        if (changedProps.ContainsKey(ROOM_TOTAL))
        {
            object valueTotal = changedProps[ROOM_TOTAL];
            int totalCount = (int)valueTotal;
            TotalEnemies.Value = totalCount;
        }

        if (changedProps.ContainsKey(ROOM_MAX_POSSIBLE))
        {
            object valueMax = changedProps[ROOM_MAX_POSSIBLE];
            int maxCount = (int)valueMax;
            MaxPossibleEnemies.Value = maxCount;
        }
    }

    public override void OnJoinedRoom()
    {
        // When we join late, grab the current numbers
        PullRoomProps();
    }
}
