using TopDown.Audio;
using TopDown.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionUI : MonoBehaviour
{
    [Header("Mission Clear")]
    [SerializeField] private GameObject missionClearScreen;
    [SerializeField] private AudioClip missionClearSound;

    public static bool IsMissionCleared { get; private set; }

    public void MissionClear()
    {
        IsMissionCleared = true;
        if (missionClearScreen) missionClearScreen.SetActive(true);
        if (missionClearSound)
            SoundManager.Instance.PlaySound(missionClearSound);

        // Stop player movement when mission clear
        var player = FindObjectOfType<PlayerMovement>();
        if (player != null)
            player.StopMovement();
    }

    public void NextMission()
    {
        if (PlayerHealthPersistence.Instance != null)
            PlayerHealthPersistence.Instance.ClearSavedHealth();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
