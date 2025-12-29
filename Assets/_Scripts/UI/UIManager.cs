using TopDown.Audio;
using TopDown.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public enum ClearScreenMode { Mission, Act }

    [Header("Scene Type")]
    [SerializeField] private ClearScreenMode clearMode = ClearScreenMode.Mission;
    [SerializeField] public int actNumber = 1;
    [SerializeField] private int levelID = 1;

    [Header("Weapon Icons")]
    [SerializeField] private GameObject[] weaponIcons;

    [Header("Game Over")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private AudioClip gameOverSound;

    [Header("Act Clear")]
    [SerializeField] private GameObject actClearedPanel;
    [SerializeField] private Text actClearedLabel;
    [SerializeField] private float actClearedDuration = 2f;


    [Header("Mission Clear")]
    [SerializeField] private GameObject missionClearScreen;
    [SerializeField] private AudioClip missionClearSound;

    [Header("Pause")]
    [SerializeField] private GameObject pauseScreen;
    public static bool IsPaused { get; private set; }
    public static bool IsCleared { get; private set; }
    public static bool IsActCleared { get; private set; }

    [Header("Settings")]
    [SerializeField] private GameObject settingUI;

    [Header("Audio Sliders")]
    [SerializeField] private Slider soundSlider;
    [SerializeField] private Slider musicSlider;

    private void Awake()
    {
        gameOverScreen.SetActive(false);
        pauseScreen.SetActive(false);
        if (missionClearScreen) missionClearScreen.SetActive(false);
        if (actClearedPanel) actClearedPanel.SetActive(false);
        IsPaused = false;
        IsCleared = false;

        if (clearMode == ClearScreenMode.Act && missionClearScreen)
            missionClearScreen.SetActive(false);
        if (clearMode == ClearScreenMode.Mission && actClearedPanel)
            actClearedPanel.SetActive(false);
    }

    private void Start()
    {
        //Lấy dữ liệu đã lưu
        float savedSound = PlayerPrefs.GetFloat("soundVolume", 1f);
        float savedMusic = PlayerPrefs.GetFloat("musicVolume", 1f);

        soundSlider.value = savedSound;
        musicSlider.value = savedMusic;

        //Apply vào audioSource để khi bắt đầu scene sẽ load đúng âm thanh 
        SoundManager.Instance.SetSoundVolume(savedSound);
        SoundManager.Instance.SetMusicVolume(savedMusic);

        // Add listeners
        soundSlider.onValueChanged.AddListener(OnSoundVolumeChanged);
        musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
    }

    public void UpdateWeaponIcon(int weaponIndex)
    {
        //Duyệt qua các icon vũ khí
        for (int i = 0; i < weaponIcons.Length; i++)
        {
            if (i == weaponIndex - 1)
            {
                //nếu chỉ số vũ khí =  chỉ số hiện tại, active icon của súng đó
                weaponIcons[i].SetActive(true);
            }
            else
            {
                weaponIcons[i].SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (TimelineController.IsCutscene)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingUI.activeInHierarchy)
                CloseSettings();
            else if (pauseScreen.activeInHierarchy)
                PauseGame(false);
            else
                PauseGame(true);
        }
    }


    public void SceneComplete()
    {
        if (clearMode == ClearScreenMode.Act)
        {
            ShowActCleared(actNumber);
        }
        else
        {
            MissionClearHandle();
        }
    }

    public void MissionClear()
    {
        if (clearMode == ClearScreenMode.Act)
        {
            ShowActCleared(actNumber);
            return;
        }
        MissionClearHandle();
    }

    public void MissionClearHandle()
    {
        IsCleared = true;
        if (actClearedPanel) actClearedPanel.SetActive(false);
        IsActCleared = false;

        if (missionClearScreen) missionClearScreen.SetActive(true);
        if (missionClearSound)
            SoundManager.Instance.PlaySound(missionClearSound);

        //Lấy component của player để ngưng di chuyển khi UI đang hiện
        var player = FindObjectOfType<Player>();
        player.StopMovement();
        SaveUnlockedLevel();
    }

    public void GameOver()
    {
        gameOverScreen.SetActive(true);
        SoundManager.Instance.PlaySound(gameOverSound);
    }
    public void Restart()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        bool isChapter;

        if (currentIndex % 3 == 0 || currentIndex % 3 == 2)
        {
            isChapter = true;
        }
        else
        {
            isChapter = false;
        }

        if (PlayerHealthPersistence.Instance != null)
        {
            if (isChapter)
                //Khởi tạo lại máu ở đầu hồi
                PlayerHealthPersistence.Instance.RestoreHealthOnCheckpoint();
            else
                // Reset lại hoàn toàn
                PlayerHealthPersistence.Instance.ClearSavedHealth();
        }
        SceneManager.LoadScene(currentIndex);
    }

    private void SaveUnlockedLevel()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        int next = levelID + 1;

        if (next > unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", next);
            PlayerPrefs.Save();
        }
    }

    public void NextMission(string sceneName)
    {
        if (PlayerHealthPersistence.Instance != null)
        {
            //Khôi phục lại toàn bộ lượng máu
            PlayerHealthPersistence.Instance.ClearSavedHealth();
        }

        //Khôi phục lại toàn bộ lượng đạn
        var ws = FindObjectOfType<WeaponSystem>();
        if (ws != null)
        {
            ws.RestoreFullAmmo();
        }

        SceneManager.LoadScene(sceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }

    #region Act
    public void ShowActCleared(int actNumber)
    {
        //Chuyển sang xử lý Mission nếu là Màn đầu
        if (clearMode == ClearScreenMode.Mission)
        {
            MissionClearHandle();
            return;
        }

        IsActCleared = true;
        IsCleared = false;

        if (actClearedLabel) actClearedLabel.text = $"Act {actNumber} Cleared!";
        if (actClearedPanel) actClearedPanel.SetActive(true);

        if (missionClearScreen) missionClearScreen.SetActive(false);

        //Reset lại bộ đếm, tránh lỗi hiển thị
        CancelInvoke(nameof(HideActCleared));
        //Chờ trong khoảng{actClearedDuration}, sau đó gọi HideActCleared 
        Invoke(nameof(HideActCleared), actClearedDuration);
    }

    public void HideActCleared()
    {
        if (actClearedPanel) actClearedPanel.SetActive(false);
        IsActCleared = false;
    }

    #endregion
    #region Pause
    public void PauseGame(bool status)
    {
        pauseScreen.SetActive(status);

        //0=time stop, 1=normal
        if (status)
            Time.timeScale = 0;

        else
            Time.timeScale = 1;
        IsPaused = status;
    }

    // public void SoundVolume()
    // {
    //     SoundManager.Instance.ChangeSoundVolume(0.2f);
    // }
    // public void MusicVolume()
    // {
    //     SoundManager.Instance.ChangeMusicVolume(0.2f);
    // }
    #endregion

    #region  SettingsUI
    public void OpenSettings()
    {
        pauseScreen.SetActive(false);
        settingUI.SetActive(true);
    }

    public void CloseSettings()
    {
        settingUI.SetActive(false);
        pauseScreen.SetActive(true);
    }

    public void OnSoundVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("soundVolume", value);
        SoundManager.Instance.SetSoundVolume(value);
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("musicVolume", value);
        SoundManager.Instance.SetMusicVolume(value);
    }
    #endregion
}
