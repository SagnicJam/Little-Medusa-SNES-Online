using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MatchConditionManager : MonoBehaviour
{
    public Button startGameButtonGO;
    public int roomId;

    public int enemyCount;
    public int enemyType;
    public int mapSelected;

    public void Initialise(int roomId)
    {
        this.roomId = roomId;
    }

    public void EnableStartGame()
    {
        startGameButtonGO.gameObject.SetActive(true);
    }

    public void DisableStartGame()
    {
        startGameButtonGO.gameObject.SetActive(false);
    }

    public void SetCount(TMP_InputField tMP_InputField)
    {
        string inputString = tMP_InputField.text;
        int amount = 0;
        if (int.TryParse(inputString,out amount))
        {
            if(amount<=10)
            {
                enemyCount = amount;
            }
            else
            {
                enemyCount = 10;
                tMP_InputField.text = 10.ToString();
                Debug.LogError("Cant be larger than 10");
            }
        }
        else
        {
            Debug.LogError("Could not parse string");
        }
    }

    public void SetEnemyType(TMP_Dropdown tMP_Dropdown)
    {
        enemyType = tMP_Dropdown.value;
    }

    public void SetMap(int mapSelected)
    {
        this.mapSelected = mapSelected;
    }

    public void StartMatch()
    {
        SignalRCoreConnect.instance.SendAsyncData<MatchBeginDto, Match>("StartMatch", new MatchBeginDto {
            matchId = roomId,
            matchConditionDto =  new MatchConditionDto
            {
                enemy = enemyType,
                enemyCount = enemyCount,
                map = mapSelected
            }
        }, onMatchStartedByRoomOwner);
    }

    void onMatchStartedByRoomOwner(Match match)
    {
        Debug.Log("Match started by room owner on match process id: "+match.ProcessID);
        MultiplayerManager.instance.DestroyMatchOptions();
    }
}
[Serializable]
public struct MatchBeginDto
{
    [field:SerializeField]
    public int matchId { get; set; }
    [field: SerializeField]
    public MatchConditionDto matchConditionDto { get; set; }
}
[Serializable]
public struct MatchConditionDto
{
    [field: SerializeField]
    public int enemy { get; set; }
    [field: SerializeField]
    public int enemyCount { get; set; }
    [field: SerializeField]
    public int map { get; set; }

    public MatchConditionDto(int enemy, int enemyCount, int map)
    {
        this.enemy = enemy;
        this.enemyCount = enemyCount;
        this.map = map;
    }
}