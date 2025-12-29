using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class TimelineController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject uiManager;
    public static bool IsCutscene { get; private set; }

    private void Start()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();

        IsCutscene = true;
        uiManager.SetActive(false);
        director.timeUpdateMode = DirectorUpdateMode.UnscaledGameTime;
        Time.timeScale = 0f;

        //gọi event OnTimelineFinished nếu timeline dừng
        director.stopped += OnTimelineFinished;
        director.Play();
    }

    private void Update()
    {
        if (IsCutscene && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipCutscene();
        }
    }

    private void SkipCutscene()
    {
        if (director == null) return;

        //nhảy tới cuối Timeline
        director.time = director.duration;
        //Update lại vị trí, trạng thái của các đối tượng trong Timeline
        director.Evaluate();
        //gọi OnTimelineFinished()
        director.Stop();
    }

    private void OnTimelineFinished(PlayableDirector obj)
    {
        IsCutscene = false;
        uiManager.SetActive(true);
        Time.timeScale = 1f;
        //hủy lắng nghe
        director.stopped -= OnTimelineFinished;
    }
}
