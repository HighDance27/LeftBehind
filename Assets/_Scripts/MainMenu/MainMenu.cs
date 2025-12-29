using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;
    }

    public void PlayCampaign()
    {
        PlayerPrefs.DeleteAll();

        if (PlayerHealthPersistence.Instance != null)
        {
            PlayerHealthPersistence.Instance.ClearSavedHealth();
        }
        SceneManager.LoadScene("Cutscene1");
    }

    public void PlayCoop()
    {
        SceneManager.LoadScene("ConnectToServer");
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void PlayIntroCutscene()
    {
        SceneManager.LoadScene("Cutscene1");
    }

    public void PlayCutscene(string sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }

    public void Back()
    {
        SceneManager.LoadScene(0);
    }
}
