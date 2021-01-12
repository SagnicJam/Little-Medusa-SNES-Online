using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct RegisterModel
{
    public string username;
    public string password;

    public RegisterModel(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}

public struct LoginModel
{
    public string username;
    public string password;

    public LoginModel(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}
[System.Serializable]
public struct LoginResponseModel
{
    public string username;
    public string token;
}
[System.Serializable]
public struct RegisterResponseModel
{
    public string username;
    public string token;
}