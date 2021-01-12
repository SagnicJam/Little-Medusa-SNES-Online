using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class PlayerList : MonoBehaviour,IPlayerList
{
    public TextMeshProUGUI roomNameText;
    public GameObject playerGO;
    public Transform playerParentTrans;
    public Dictionary<string, PlayerInfo> playerInfoDic = new Dictionary<string, PlayerInfo>();

    public Room room;

    void OnEnable()
    {
        SignalRCoreConnect.instance._connection.On<PlayerInfoData>(nameof(OnPlayerJoinedRoom), OnPlayerJoinedRoom);
        SignalRCoreConnect.instance._connection.On<PlayerInfoData>(nameof(OnPlayerLeftRoom), OnPlayerLeftRoom);
        SignalRCoreConnect.instance._connection.On<Match>(nameof(OnMatchFound), OnMatchFound);
    }

    void OnDisable()
    {
        SignalRCoreConnect.instance._connection.Remove(nameof(OnPlayerJoinedRoom));
        SignalRCoreConnect.instance._connection.Remove(nameof(OnPlayerLeftRoom));
        SignalRCoreConnect.instance._connection.Remove(nameof(OnMatchFound));
    }

    public void DisplayAllPlayerList(Room room)
    {
        this.room = room;
        roomNameText.text = room.RoomName;
        foreach(KeyValuePair<string,PlayerInfoData>kvp in room.playerList)
        {
            GameObject playerGORef = Instantiate(playerGO, playerParentTrans, false);
            PlayerInfo playerInfo = playerGORef.GetComponent<PlayerInfo>();
            playerInfo.Initialise(kvp.Value);
            playerInfoDic.Add(kvp.Key, playerInfo);
        }
    }

    public void ReturnToLobby()
    {
        LobbyScreen.instance.LeaveRoom(room);
    }


    void AddNewPlayer(PlayerInfoData playerInfoData)
    {
        if(playerInfoDic.ContainsKey(playerInfoData.connectionId))
        {
            Debug.LogError("Already containing connection id for player: "+playerInfoData.Name);
        }
        else
        {
            GameObject playerGORef = Instantiate(playerGO, playerParentTrans, false);
            PlayerInfo playerInfo = playerGORef.GetComponent<PlayerInfo>();
            playerInfo.Initialise(playerInfoData);
            playerInfoDic.Add(playerInfoData.connectionId, playerInfo);
        }
    }

    void RemovePlayer(PlayerInfoData playerInfoData)
    {
        if (playerInfoDic.ContainsKey(playerInfoData.connectionId))
        {
            PlayerInfo playerInfo = playerInfoDic[playerInfoData.connectionId];
            Destroy(playerInfo.gameObject);
            playerInfoDic.Remove(playerInfoData.connectionId);
        }
        else
        {
            Debug.LogError("No player found to remove "+playerInfoData.Name);
        }
    }

    public void OnPlayerJoinedRoom(PlayerInfoData player)
    {
        Debug.Log("New player joined room : " + player.Name);
        AddNewPlayer(player);
    }

    public void OnPlayerLeftRoom(PlayerInfoData player)
    {
        Debug.Log("Player Left room : " + player.Name);
        RemovePlayer(player);
    }

    public void OnMatchFound(Match match)
    {
        Debug.Log("Match found: "+match.MatchID);
        Loader loader = null;
        loader = Instantiate(MultiplayerManager.instance.loader, MultiplayerManager.instance.Canvas, false);
        loader.SetMessage("Match found Successfully!");
        loader.transform.SetAsLastSibling();
    }
}
