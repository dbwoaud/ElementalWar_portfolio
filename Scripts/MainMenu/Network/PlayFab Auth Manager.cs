using System;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabAuthManager : MonoBehaviour
{
    public event Action<string> OnLoginSuccessEvent;
    public event Action<string> OnLoginErrorEvent;
    public event Action OnRegisterSuccessEvent;
    public event Action<string> OnRegisterErrorEvent;


    public void PlayFabLogin(string email, string password) // 플레이팹에 로그인하는 함수
    {
        var request = new LoginWithEmailAddressRequest { Email = email, Password = password };
        PlayFabClientAPI.LoginWithEmailAddress(request, OnPlayFabLoginSuccess, OnPlayFabLoginError);
    }

    private void OnPlayFabLoginSuccess(LoginResult result) // 로그인 성공 시 실행되는 함수
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetAccountInfoSuccess, OnPlayFabLoginError);
    }

    private void OnPlayFabLoginError(PlayFabError error) // 로그인 실패 시 실행되는 함수
    {
        string translatedError = ErrorTranslator.GetPlayFabErrorMessage(error.Error);
        OnLoginErrorEvent?.Invoke(translatedError);
    }

    private void OnGetAccountInfoSuccess(GetAccountInfoResult infoResult) // 이메일과 비밀번호를 통해 닉네임을 불러오는 함수 
    {
        string nickname = PlayerConstants.Default.Nickname;
        if (infoResult.AccountInfo != null && infoResult.AccountInfo.Username != null)
        {
            nickname = infoResult.AccountInfo.Username;
        }

        OnLoginSuccessEvent?.Invoke(nickname);
    }

    public void PlayFabRegister(string email, string password, string nickname) // 플레이팹에 회원가입하는 함수
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            Username = nickname,
            RequireBothUsernameAndEmail = true
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, OnPlayFabRegisterSuccess, OnPlayFabRegisterError);
    }

    private void OnPlayFabRegisterSuccess(RegisterPlayFabUserResult result) // 회원가입 성공 시 실행되는 함수
    {
       OnRegisterSuccessEvent?.Invoke();
    }

    private void OnPlayFabRegisterError(PlayFabError error) // 회원가입 실패 시 실행되는 함수
    {
        string translatedError = ErrorTranslator.GetPlayFabErrorMessage(error.Error);
        OnRegisterErrorEvent?.Invoke(translatedError);
    }
}
