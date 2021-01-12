using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public static AccountManager instance;
    public string IP = "http://localhost:5000/api";
    private void Awake()
    {
        instance = this;
    }

    
}
