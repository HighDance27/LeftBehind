using UnityEngine;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class PlayfabManager : MonoBehaviour
{
    [Header("UI")]
    public Text messageText;

    [Header("UI - Login")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;

    [Header("UI - Register")]
    public TMP_InputField registerDisplayNameInput;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;

    private Coroutine messageCoroutine;
    private bool isHeartbeatStarted = false;

    public static PlayfabManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Instance.loginEmailInput = this.loginEmailInput;
            Instance.loginPasswordInput = this.loginPasswordInput;
            Instance.registerDisplayNameInput = this.registerDisplayNameInput;
            Instance.registerEmailInput = this.registerEmailInput;
            Instance.registerPasswordInput = this.registerPasswordInput;
            Instance.messageText = this.messageText;

            Destroy(gameObject);
        }
    }

    #region Login & Register

    public void RegisterButton()
    {
        if (registerDisplayNameInput.text.Length < 3)
        {
            ShowMessage("Username must have at least 3 characters");
            return;
        }

        if (registerPasswordInput.text.Length < 6)
        {
            ShowMessage("Password must have at least 6 characters");
            return;
        }

        var request = new RegisterPlayFabUserRequest
        {
            Username = registerDisplayNameInput.text,
            Email = registerEmailInput.text,
            Password = registerPasswordInput.text,
            RequireBothUsernameAndEmail = true
        };
        PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
    }

    public void LoginButton()
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = loginEmailInput.text,
            Password = loginPasswordInput.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams //gửi yêu cầu trả về thông tin tài khoản
            {
                GetUserAccountInfo = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
    }

    public void ResetPasswordButton()
    {
        var request = new SendAccountRecoveryEmailRequest
        {
            Email = loginEmailInput.text,
            TitleId = "1BB88D"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        var updateRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = registerDisplayNameInput.text
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(updateRequest,
            res => ShowMessage("Register Success! Log in and Play"), OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        PlayerPrefs.SetString("PlayFabId", result.PlayFabId);
        PlayerPrefs.SetString("SavedEmail", loginEmailInput.text);
        string displayName = result.InfoResultPayload.AccountInfo.TitleInfo.DisplayName;
        PlayerPrefs.SetString("SavedUsername", displayName);
        CheckAccountStatus();
    }

    void CheckAccountStatus()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, OnCheckStatusSuccess, OnError);
    }

    void OnCheckStatusSuccess(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("LastActive"))
        {
            long lastActive = long.Parse(result.Data["LastActive"].Value);
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Nếu hoạt động cách đây chưa tới 20 giây -> Chặn đăng nhập
            if (currentTime - lastActive < 20)
            {
                ShowMessage("Account is already in use on another device!");
                PlayerPrefs.DeleteKey("PlayFabId");
                return;
            }
        }

        ProceedToGame();
    }
    #endregion

    void ProceedToGame()
    {
        ShowMessage("Login Successfully!");
        LoadLevelProgressFromCloud();

        if (!isHeartbeatStarted)
        {
            StartCoroutine(HeartbeatRoutine());
            isHeartbeatStarted = true;
        }
        SceneManager.LoadScene("ConnectToServer");
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        ShowMessage("Password reset has been sent to your email!");
    }

    void OnError(PlayFabError error)
    {
        ShowMessage(error.ErrorMessage);
        Debug.Log(error.GenerateErrorReport());
    }

    void ShowMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;

        // Chỉ dừng coroutine của tin nhắn, không dừng Heartbeat
        if (messageCoroutine != null) StopCoroutine(messageCoroutine);
        messageCoroutine = StartCoroutine(ClearMessageAfterDelay(2f));
    }

    IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null) messageText.text = "";
        messageCoroutine = null;
    }

    public void ClearInputFieldOnBack()
    {
        loginEmailInput.text = "";
        loginPasswordInput.text = "";

        registerDisplayNameInput.text = "";
        registerEmailInput.text = "";
        registerPasswordInput.text = "";

        messageText.text = "";
    }

    public void LoadLevelProgressFromCloud()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, OnDataReceived, OnError);
    }

    void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("UnlockedLevel"))
        {
            int cloudLevel = int.Parse(result.Data["UnlockedLevel"].Value);

            // Lưu vào máy để LevelSelectManager sử dụng
            PlayerPrefs.SetInt("UnlockedLevel", cloudLevel);
            PlayerPrefs.Save();

            Debug.Log("Đã tải và đồng bộ UnlockedLevel: " + cloudLevel);
        }
        else
        {
            // Nếu người chơi mới chưa có dữ liệu trên Cloud, mặc định là màn 1
            PlayerPrefs.SetInt("UnlockedLevel", 1);
            PlayerPrefs.Save();
            Debug.Log("Người chơi mới: Mặc định UnlockedLevel = 1");
        }
    }

    IEnumerator HeartbeatRoutine()
    {
        //Lặp từ lúc login cho đến khi tắt game
        while (true)
        {
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> {
                    {
                        "LastActive", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
                    }
                }
            };
            PlayFabClientAPI.UpdateUserData(request, null, null);
            yield return new WaitForSeconds(10f);
        }
    }

    public void StopHeartbeat()
    {
        if (isHeartbeatStarted)
        {
            StopAllCoroutines();
            isHeartbeatStarted = false;
            messageCoroutine = null;
            Debug.Log("Heartbeat has been stopped safely.");
        }
    }

    private void OnApplicationQuit()
    {
        if (PlayFabClientAPI.IsClientLoggedIn())
        {
            var request = new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { "LastActive", "0" } }
            };
            PlayFabClientAPI.UpdateUserData(request, null, null);
        }
    }

}
