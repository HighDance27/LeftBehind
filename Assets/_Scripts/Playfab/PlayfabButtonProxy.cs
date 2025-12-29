using UnityEngine;

public class PlayfabButtonProxy : MonoBehaviour
{
    //Proxy giúp 2 nút luôn tìm đúng "Manager thật" đang sống, 
    // thay vì bám vào cái "Manager giả" vừa bị Singleton Destroy
    public void Login()
    {
        if (PlayfabManager.Instance != null)
            PlayfabManager.Instance.LoginButton();
    }

    public void Register()
    {
        if (PlayfabManager.Instance != null)
            PlayfabManager.Instance.RegisterButton();
    }
}