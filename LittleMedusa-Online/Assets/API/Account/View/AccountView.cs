using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AccountView : MonoBehaviour
{
    public AccountController accountController;

    public TMP_InputField registerUsernameIF;
    public TMP_InputField registerPasswordIF;
    public TMP_InputField loginUsernameIF;
    public TMP_InputField loginPasswordIF;

    public RegisterResponseModel registerResponseModel=new RegisterResponseModel();
    public LoginResponseModel loginResponseModel=new LoginResponseModel();

    public void Register()
    {
        RegisterModel registerModel = new RegisterModel(registerUsernameIF.text, registerPasswordIF.text);
        accountController.Register(registerModel, OnRegisterResponseReceived);
    }

    public void Login()
    {
        LoginModel loginModel = new LoginModel(loginUsernameIF.text, loginPasswordIF.text);
        accountController.Login(loginModel, OnLoginResponseReceived);
    }

    public void OnRegisterResponseReceived(string response,string error, long statusCode)
    {
        if (!string.IsNullOrEmpty(error))
        {
            Debug.LogError(statusCode);
            Debug.LogError(response);
        }
        else
        {
            registerResponseModel = JsonUtility.FromJson<RegisterResponseModel>(response);
        }
    }

    public void OnLoginResponseReceived(string response, string error, long statusCode)
    {
        if(!string.IsNullOrEmpty(error))
        {
            Debug.LogError(statusCode);
            Debug.LogError(response);
        }
        else
        {
            loginResponseModel=JsonUtility.FromJson<LoginResponseModel>(response);
        }
    }
}
