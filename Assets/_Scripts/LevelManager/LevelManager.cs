using TopDown.Shooting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        SaveAllWeaponsAmmo(collision.gameObject);
        SavePlayerHealth(collision.gameObject);
        UnlockNewLevel();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void SaveAllWeaponsAmmo(GameObject player)
    {
        var ws = player.GetComponentInChildren<WeaponSystem>(true);
        if (ws == null || ws.weapons == null) return;

        PlayerPrefs.SetInt("Ammo_Count", ws.weapons.Length);

        for (int i = 0; i < ws.weapons.Length; i++)
        {
            var w = ws.weapons[i];
            if (w == null) continue;

            PlayerPrefs.SetInt($"Ammo_{i}_Total", w.TotalAmmo.Value);
            PlayerPrefs.SetInt($"Ammo_{i}_Clip", w.CurrentAmmoInClip.Value);
        }

        PlayerPrefs.Save();
    }

    private void SavePlayerHealth(GameObject player)
    {
        var health = player.GetComponent<HealthController>();
        if (health == null) return;

        PlayerPrefs.SetInt("HP_Saved", health.CurrentHealth);
        PlayerPrefs.SetInt("HP_SavedMax", health.MaximumHealth);
        PlayerPrefs.Save();
    }

    private void UnlockNewLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int maxUnlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        // Nếu màn vừa chơi xong là màn cao nhất đang mở
        // Thì mới mở khóa màn tiếp theo.

        if (currentSceneIndex >= maxUnlockedLevel)
        {
            int nextLevel = maxUnlockedLevel + 1;

            PlayerPrefs.SetInt("UnlockedLevel", nextLevel);

            PlayerPrefs.SetInt("ReachedIndex", currentSceneIndex + 1);

            PlayerPrefs.Save();
            Debug.Log("Đã mở khóa Level: " + nextLevel);
        }
        else
        {
            Debug.Log($"Chơi lại màn cũ ({currentSceneIndex}). Giữ nguyên UnlockedLevel ({maxUnlockedLevel}).");
        }
    }
}
