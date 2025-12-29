using UnityEngine;
using UnityEngine.SceneManagement;

public class ToMenuWhenCleared : MonoBehaviour
{
    public void DeleteKeyAndLoadMenu()
    {
        if (PlayerHealthPersistence.Instance != null)
        {
            PlayerHealthPersistence.Instance.ClearSavedHealth();
        }

        var ws = FindObjectOfType<WeaponSystem>();
        if (ws != null)
        {
            ws.RestoreFullAmmo();
        }
        SceneManager.LoadScene(0);
    }
}
