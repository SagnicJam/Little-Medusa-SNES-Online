using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public ServerSideGameManager serverSideGameManager;
    public ClientSideGameManager clientSideGameManager;

    public static MultiplayerManager instance;

    public bool isServer;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if(isServer)
        {
            Instantiate(serverSideGameManager);
        }
        else
        {
            Instantiate(clientSideGameManager);
        }
    }
}
