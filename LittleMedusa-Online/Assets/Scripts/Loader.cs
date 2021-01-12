using UnityEngine;
using TMPro;

public class Loader : MonoBehaviour
{
    public GameObject loaderGO;
    public GameObject messageGO;
    public TextMeshProUGUI messageUI;

    public void SetMessage(string message)
    {
        loaderGO.SetActive(false);
        messageGO.SetActive(true);
        messageUI.text = message;
        Debug.Log("setting message");
    }

    public void StartLoading()
    {
        loaderGO.SetActive(true);
        messageGO.SetActive(false);
        Debug.Log("Loading");
    }

    public void Close()
    {
        Destroy(gameObject);
    }
}
