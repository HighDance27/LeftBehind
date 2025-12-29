using System.Collections;
using Photon.Pun;
using TopDown.Audio;
using TopDown.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayFab;
using System.Collections.Generic;
using PlayFab.ClientModels;

public class PlayerHUD : MonoBehaviourPunCallbacks
{
    [Header("Weapon Icons")]
    [SerializeField] private GameObject[] weaponIcons;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private GameObject waitForOthersText;
    [SerializeField] private GameObject restartButton;

    [Header("Mission Clear")]
    [SerializeField] public GameObject missionClearScreen;
    [SerializeField] private AudioClip missionClearSound;
    [SerializeField] private GameObject nextLevelButton;
    [SerializeField] private GameObject waitForHost;

    [Header("Pause")]
    [SerializeField] private GameObject pauseScreen;
    public static bool IsPaused { get; private set; }
    public static bool IsCleared { get; private set; }
    public static bool IsGameOver { get; private set; }

    private bool _checkingAllDead;

    [Header("Settings")]
    [SerializeField] private GameObject settingUI;

    [Header("Audio Sliders")]
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;

    [Header("Player Progress")]
    private int currentLevelNumber;

    private void Awake()
    {
        MPSceneEnemyManager sem = FindObjectOfType<MPSceneEnemyManager>();
        if (sem != null)
            currentLevelNumber = sem.levelNumber;
        var pv = GetComponentInParent<PhotonView>();
        if (pv != null && !pv.IsMine)
        {
            gameObject.SetActive(false); //tắt hud của người khác
            return;
        }

        if (missionClearScreen) missionClearScreen.SetActive(false);
        if (waitForHost) waitForHost.SetActive(false);
        if (nextLevelButton) nextLevelButton.SetActive(false);

        if (gameOverScreen) gameOverScreen.SetActive(false);
        if (pauseScreen) pauseScreen.SetActive(false);
        if (missionClearScreen) missionClearScreen.SetActive(false);
        if (waitForOthersText) waitForOthersText.SetActive(false);
        if (restartButton)
            restartButton.SetActive(PhotonNetwork.IsMasterClient);

        IsPaused = false;
        IsCleared = false;
        IsGameOver = false;
    }

    private void Start()
    {
        float savedSound = PlayerPrefs.GetFloat("soundVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("musicVolume", 1f);

        if (soundSlider) soundSlider.value = savedSound;
        if (musicSlider) musicSlider.value = savedMusic;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetSoundVolume(savedSound);
            SoundManager.Instance.SetMusicVolume(savedMusic);
        }

        if (soundSlider) soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseScreen != null && pauseScreen.activeInHierarchy)
                PauseGame(false);
            else if (settingUI != null && settingUI.activeInHierarchy)
                CloseSettings();
            else
                PauseGame(true);
        }
    }

    public void UpdateWeaponIcon(int weaponIndex)
    {
        if (weaponIndex <= 0 || weaponIcons == null) return;
        for (int i = 0; i < weaponIcons.Length; i++)
            weaponIcons[i].SetActive(i == weaponIndex - 1);
    }

    public void GameOver()
    {
        int alivePlayers = 0;

        foreach (var h in SpawnPlayers.ActivePlayers)
        {
            if (h != null && h.CurrentHealth > 0)
            {
                alivePlayers++;
            }
        }

        //nếu vẫn còn 1 Player còn sống, hiện wait text
        if (alivePlayers > 0)
        {
            if (waitForOthersText) waitForOthersText.SetActive(true);
            if (restartButton) restartButton.SetActive(false);
        }
        else
        {
            //Player chết hết
            if (waitForOthersText) waitForOthersText.SetActive(false);
            if (restartButton)
                restartButton.SetActive(PhotonNetwork.IsMasterClient);
        }

        if (PhotonNetwork.IsMasterClient && !_checkingAllDead)
        {
            _checkingAllDead = true;
            StartCoroutine(CheckAllPlayersDeadRoutine());
        }

        if (gameOverScreen) gameOverScreen.SetActive(true);
        if (SoundManager.Instance != null) SoundManager.Instance.PlaySound(gameOverSound);
        IsGameOver = true;
    }

    private IEnumerator CheckAllPlayersDeadRoutine()
    {
        while (true)
        {
            int alivePlayers = 0;
            foreach (var h in SpawnPlayers.ActivePlayers)
            {
                if (h != null && h.CurrentHealth > 0)
                    alivePlayers++;
            }

            if (alivePlayers == 0)
            {
                if (waitForOthersText) waitForOthersText.SetActive(false);

                if (restartButton)
                    restartButton.SetActive(PhotonNetwork.IsMasterClient);

                yield break; // stop checking
            }

            //check again later
            yield return new WaitForSeconds(0.5f);
        }
    }

    public void RestartLevel()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (NetworkGameOver.Instance != null)
        {
            NetworkGameOver.Instance.RequestRestart();
        }
        else
        {
            Debug.LogError("NetworkGameManager.Instance is null. Make sure it exists in the scene!");
        }
    }

    public void MissionClear()
    {
        MissionClearHandle();
    }

    public void MissionClearHandle()
    {
        if (gameOverScreen) gameOverScreen.SetActive(false);

        IsCleared = true;

        if (missionClearScreen) missionClearScreen.SetActive(true);
        if (SoundManager.Instance != null && missionClearSound)
            SoundManager.Instance.PlaySound(missionClearSound);

        bool isMaster = PhotonNetwork.IsMasterClient;

        // Master: see Next Level
        if (nextLevelButton)
            nextLevelButton.SetActive(isMaster);

        // Non-Master: see Wait For Host
        if (waitForHost)
            waitForHost.SetActive(!isMaster);

        SaveLevelProgress();
    }

    private void SaveLevelProgress()
    {
        //Lấy level cao nhất mà người chơi đã mở khóa từ máy (hoặc từ PlayFab)
        int currentSavedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // CHỈ tăng lên màn tiếp theo nếu vừa thắng màn chơi cao nhất đang có.
        if (currentLevelNumber == currentSavedLevel)
        {
            int nextLevel = currentSavedLevel + 1;
            //Lưu trên máy
            PlayerPrefs.SetInt("UnlockedLevel", nextLevel);
            PlayerPrefs.Save();

            //Gửi lên PlayFab
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string>
            {
                { "UnlockedLevel", nextLevel.ToString() }
            }
            };
            PlayFabClientAPI.UpdateUserData(request,
               result => Debug.Log($"Đã mở khóa màn {nextLevel} trên Cloud"),
               error => Debug.LogError("Lỗi lưu PlayFab: " + error.GenerateErrorReport())
           );
        }
        else
        {
            // Nếu thắng màn 3 khi mới chỉ ở màn 1, hoặc chơi lại màn cũ.
            Debug.Log($"[Progression] Thắng màn {currentLevelNumber} nhưng tiến trình {currentSavedLevel} được giữ nguyên.");
        }
    }

    public void PauseGame(bool status)
    {
        if (pauseScreen) pauseScreen.SetActive(status);

        IsPaused = status;
    }

    public void OpenSettings()
    {
        if (pauseScreen) pauseScreen.SetActive(false);
        if (settingUI) settingUI.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingUI) settingUI.SetActive(false);
        if (pauseScreen) pauseScreen.SetActive(true);
    }

    public void OnSoundVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("soundVolume", value);
        if (SoundManager.Instance != null) SoundManager.Instance.SetSoundVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("musicVolume", value);
        if (SoundManager.Instance != null) SoundManager.Instance.SetMusicVolume(value);
    }

    public void SetWeaponSystem(MPWeaponSystem ws)
    {
        var counter = GetComponentInChildren<MPAmmoCounter>(true);
        if (counter != null)
        {   //Truyền tham chiếu vào cho MPAmmoCounter để theo dõi đạn
            counter.SetWeaponSystem(ws);
        }
    }

    public void BackToLobby()
    {
        IsPaused = false;
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            SceneManager.LoadScene("Lobby");
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Lobby");
    }
    public void NextLevel()
    {
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
