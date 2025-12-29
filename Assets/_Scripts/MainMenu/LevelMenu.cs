using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenu : MonoBehaviour
{
    [SerializeField] private Button[] buttons;
    [SerializeField] private string[] levelSceneNames;

    private void Awake()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].interactable = false;
        }

        int max = Mathf.Min(unlockedLevel, buttons.Length);
        for (int i = 0; i < max; i++)
        {
            buttons[i].interactable = true;
        }
    }

    public void OpenLevel(int levelIndex)
    {
        // Vì mảng bắt đầu từ 0, nên ta lấy levelIndex - 1 để lấy tên Scene
        if (levelIndex - 1 >= levelSceneNames.Length)
        {
            Debug.LogError("Chưa điền tên Scene vào mảng levelSceneNames hoặc sai số thứ tự!");
            return;
        }
        //vd: OpenLevel(2) thì levelIndex là 2-1=1 -> load màn 2 
        string sceneToLoad = levelSceneNames[levelIndex - 1];
        int maxUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);

        if (levelIndex < maxUnlocked)
        {
            // Chơi lại màn cũ -> Reset
            PlayerPrefs.DeleteKey("HP_Saved");
            PlayerPrefs.DeleteKey("HP_SavedMax");
            PlayerPrefs.DeleteKey("Ammo_Count");
            Debug.Log("Reset dữ liệu vì chơi lại màn cũ.");
        }
        SceneManager.LoadScene(sceneToLoad);
    }
}
