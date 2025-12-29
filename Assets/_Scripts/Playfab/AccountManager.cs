using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;

public class AccountManager : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public Button resetPassword;

    public Text messageText;
    private Coroutine messageCoroutine;

    private void Start()
    {
        usernameInput.text = PlayerPrefs.GetString("SavedUsername", "");
    }

    public void OnClickSave()
    {
        if (!string.IsNullOrEmpty(usernameInput.text))
        {
            if (usernameInput.text.Length < 3)
            {
                ShowMessage("Username too short!");
            }

            var request = new UpdateUserTitleDisplayNameRequest { DisplayName = usernameInput.text };
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUsernameSuccess, OnError);
        }
    }

    void OnUsernameSuccess(UpdateUserTitleDisplayNameResult result)
    {
        PlayerPrefs.SetString("SavedUsername", result.DisplayName);
        PlayerPrefs.Save();

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.NickName = result.DisplayName;
        }

        ShowMessage("Username updated successfully!");
    }

    public void OnClickResetPassword()
    {
        string savedEmail = PlayerPrefs.GetString("SavedEmail", "");

        var request = new SendAccountRecoveryEmailRequest
        {
            Email = savedEmail,
            TitleId = PlayFabSettings.staticSettings.TitleId
        };

        PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
    }

    void OnPasswordReset(SendAccountRecoveryEmailResult result)
    {
        ShowMessage("Password reset has been sent to your email!");
    }

    void OnError(PlayFabError error)
    {
        ShowMessage(error.ErrorMessage);
    }

    void ShowMessage(string msg)
    {
        if (messageText == null) return;

        messageText.text = msg;
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(ClearMessageAfterDelay(2f));
    }

    IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (messageText != null) messageText.text = "";
        messageCoroutine = null;
    }

    public void OnBackButton()
    {
        SceneManager.LoadScene("Lobby");
    }

    public void ResetUnlockedLevelButton()
    {
        PlayerPrefs.SetInt("UnlockedLevel", 1);
        PlayerPrefs.Save();

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
                {
                    { "UnlockedLevel", "1" }
                }
        };

        PlayFabClientAPI.UpdateUserData(request, OnResetDataSuccess, OnError);
    }

    void OnResetDataSuccess(UpdateUserDataResult result)
    {
        Debug.Log("[PlayFab] Dữ liệu đã được đưa về màn 1.");
    }
}
