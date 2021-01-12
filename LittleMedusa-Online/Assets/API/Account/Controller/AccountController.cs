using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
public class AccountController : MonoBehaviour,IAccount
{

    public void Login(LoginModel loginModel, OnWorkDone<string,string,long> onWorkDone)
    {
        IEnumerator ie = ProcessPostRequest(JsonUtility.ToJson(loginModel), "login",onWorkDone);
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    public void Register(RegisterModel registerModel, OnWorkDone<string,string, long> onWorkDone)
    {
        IEnumerator ie = ProcessPostRequest(JsonUtility.ToJson(registerModel), "register",onWorkDone);
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    IEnumerator ProcessPostRequest(string json,string type, OnWorkDone<string,string, long> onWorkDone)
    {
        var request = new UnityWebRequest(AccountManager.instance.IP+"/account"+"/"+ type, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.isNetworkError)
        {
            onWorkDone.Invoke("IsNetworkError",request.error,request.responseCode);
        }
        if (request.isHttpError)
        {
            onWorkDone.Invoke(request.downloadHandler.text, request.error, request.responseCode);
        }
        else
        {
            onWorkDone.Invoke(request.downloadHandler.text, request.error, request.responseCode);
        }
    }
}
