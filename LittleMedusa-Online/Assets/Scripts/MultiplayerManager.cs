using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public Transform Canvas;

    public ServerSideGameManager serverSideGameManager;
    public ClientSideGameManager clientSideGameManager;

    public CharacterSelectionScreen characterSelectionScreen;

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
            Instantiate(characterSelectionScreen, Canvas, false);
        }
    }
}
