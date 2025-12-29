using UnityEngine;
using UnityEngine.Playables;

public class TimelineTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject boss;
    [SerializeField] private Transform door;
    [SerializeField] private Transform closedPos;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private GameObject ui;

    private bool isPlayingTimeline = false;

    private void Awake()
    {
        if (director == null)
            director = GetComponent<PlayableDirector>();
        director.stopped += OnTimelineFinished;
    }

    private void Update()
    {
        if (isPlayingTimeline && Input.GetKeyDown(KeyCode.Escape))
        {
            SkipTimeline();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        player.SetActive(false);
        boss.SetActive(false);
        ui.SetActive(false);
        director.gameObject.SetActive(true);
        director.Play();
        isPlayingTimeline = true;

    }

    private void OnTimelineFinished(PlayableDirector d)
    {
        isPlayingTimeline = false;

        door.position = closedPos.position;
        director.gameObject.SetActive(false);

        if (respawnPoint != null)
            player.transform.position = respawnPoint.position;

        player.SetActive(true);
        boss.SetActive(true);
        ui.SetActive(true);
    }

    public void SkipTimeline()
    {
        director.Stop();
        OnTimelineFinished(director);
    }
}
