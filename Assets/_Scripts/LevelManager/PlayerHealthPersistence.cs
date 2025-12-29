using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealthPersistence : MonoBehaviour
{
    public static PlayerHealthPersistence Instance { get; private set; }

    private HealthController healthController;

    private const string KEY_SAVED_HP = "HP_Saved";
    private const string KEY_SAVED_MAX_HP = "HP_SavedMax";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        healthController = GetComponent<HealthController>();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //khi load scene mới, đặt lại máu từ PlayerPrefs
        StartCoroutine(ApplySavedHealth());
    }

    private IEnumerator ApplySavedHealth()
    {
        //Đợi 1 frame cho component HealthController của player khởi tạo xong
        yield return null;

        if (healthController == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                healthController = player.GetComponent<HealthController>();
        }

        if (healthController == null)
            yield break;

        if (PlayerPrefs.HasKey(KEY_SAVED_HP))
        {
            int savedHP = PlayerPrefs.GetInt(KEY_SAVED_HP);
            int savedMax = PlayerPrefs.GetInt(KEY_SAVED_MAX_HP, healthController.MaximumHealth);

            //giới hạn để không cho vượt quá máu tối đa
            savedHP = Mathf.Clamp(savedHP, 0, savedMax);
            healthController.SetHealth(savedHP);
        }
        else
        {
            //nếu không có giá trị đã lưu, đặt lại = maxHP
            healthController.SetHealth(healthController.MaximumHealth);
        }
    }

    public void RestoreHealthOnCheckpoint()
    {
        if (healthController != null)
            healthController.SetHealth(healthController.MaximumHealth);
    }

    public void ClearSavedHealth()
    {
        PlayerPrefs.DeleteKey(KEY_SAVED_HP);
        PlayerPrefs.DeleteKey(KEY_SAVED_MAX_HP);
        PlayerPrefs.Save();
    }
}
