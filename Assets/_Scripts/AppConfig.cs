using UnityEngine;

public static class AppConfig
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyGameSettings()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 144;
        Application.runInBackground = true;
    }
}