using UnityEngine;
using UnityEngine.SceneManagement;
namespace TopDown.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Music Clips")]
        [SerializeField] private AudioClip mainMenuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        private AudioSource soundSource;
        private AudioSource musicSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            soundSource = GetComponent<AudioSource>();
            musicSource = transform.GetChild(0).GetComponent<AudioSource>();

            //lắng nghe khi scene thay đổi
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void PlaySound(AudioClip sound)
        {
            soundSource.PlayOneShot(sound);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {

            if (scene.buildIndex == 0 || scene.name.StartsWith("Cutscene") ||
            scene.name == "Lobby" || scene.name == "ConnectToServer" || scene.name == "AccountManagement")
            {
                PlayMusic(mainMenuMusic);
            }
            else
            {
                PlayMusic(gameplayMusic);
            }
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null)
            {
                Debug.Log("NO SFX");
                return;
            }
            //không phát nhiều lần đoạn nhạc
            if (musicSource.clip == clip && musicSource.isPlaying)
                return;

            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }

        // Slider controls
        public void SetSoundVolume(float value)
        {
            SetSourceVolume(1f, "soundVolume", value, soundSource);
        }

        public void SetMusicVolume(float value)
        {
            SetSourceVolume(0.3f, "musicVolume", value, musicSource);
        }

        private void SetSourceVolume(float baseVolume, string volumeName, float value, AudioSource source)
        {
            float v = Mathf.Clamp01(value);
            source.volume = v * baseVolume;
            PlayerPrefs.SetFloat(volumeName, v);
        }
    }
}
