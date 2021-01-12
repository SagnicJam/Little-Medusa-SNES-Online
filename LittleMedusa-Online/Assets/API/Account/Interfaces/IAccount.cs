using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public delegate void OnWorkDone<T,X, V>(T data,X data3, V data2);
public delegate void OnWorkDone<T>(T data);
public delegate void OnWorkDone ();
public interface IAccount
{
    void Register(RegisterModel registerModel,OnWorkDone<string, string , long> onWorkDone);
    void Login(LoginModel loginModel, OnWorkDone<string, string , long> onWorkDone);
}
