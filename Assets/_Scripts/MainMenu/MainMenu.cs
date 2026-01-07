using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject levelButtonsGroup;
    public GameObject levelPanel;
    public GameObject mainPanel;
    public GameObject playFabPanel;

    void Start()
    {
        Cursor.visible = true;
    }

    public void OpenLevelPanel()
    {
        levelPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void CloseLevelPanel()
    {
        levelPanel.SetActive(false);
        mainPanel.SetActive(true);
    }

    public void OpenActPanel(GameObject panelToOpen)
    {
        if (levelButtonsGroup != null)
        {
            levelButtonsGroup.SetActive(false);
        }

        if (panelToOpen != null)
        {
            panelToOpen.SetActive(true);
        }
    }

    public void BackToLevelButtons(GameObject currentActPanel)
    {
        if (currentActPanel != null)
        {
            currentActPanel.SetActive(false);
        }

        if (levelButtonsGroup != null)
        {
            levelButtonsGroup.SetActive(true);
        }
    }

    public void OpenPlayFabPanel()
    {
        playFabPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void ClosePlayFabPanel()
    {
        playFabPanel.SetActive(false);
        mainPanel.SetActive(true);
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

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName.ToString());
    }

    public void Back()
    {
        SceneManager.LoadScene(0);
    }
}
