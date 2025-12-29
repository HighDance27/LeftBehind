using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class ShowEndText : MonoBehaviour
{
    public PlayableDirector director;
    public string sceneToLoad = "_MainMenu";
    private bool hasSkipped = false;

    private void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        director.stopped += OnTimelineFinished;
    }

    private void Update()
    {
        if (!hasSkipped && Input.GetKeyDown(KeyCode.Escape))
            Skip();
    }

    private void Skip()
    {
        if (director == null) return;

        hasSkipped = true;
        director.time = 60f;
        director.Evaluate();
    }

    private void OnTimelineFinished(PlayableDirector d)
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
