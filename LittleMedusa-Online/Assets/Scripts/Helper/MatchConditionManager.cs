using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class MatchConditionManager : MonoBehaviour
{
    public StageSelection stageSelection;
    public TMP_InputField enemyCountInputFieldText;
    public TMP_Dropdown enemyType_tMP_Dropdown;

    public Button startGameButtonGO;
    public int roomId;

    public int enemyCountPrevious;
    public int enemyTypePrevious;
    public int mapSelectedPrevious;

    public int enemyCount;
    public int enemyType;
    public int mapSelected;

    public void Initialise(int roomId)
    {
        this.roomId = roomId;
        enemyCount = 1;
        enemyType = 0;
        mapSelected = 0;
    }

    public void OpenPrematchConditionMenu()
    {
        enemyCountPrevious = enemyCount;
        enemyTypePrevious = enemyType;

        enemyType_tMP_Dropdown.SetValueWithoutNotify(enemyType);
        enemyCountInputFieldText.text = enemyCount.ToString();
    }

    public void SetCount(TMP_InputField tMP_InputField)
    {
        string inputString = tMP_InputField.text;
        int amount = 0;
        if (int.TryParse(inputString, out amount))
        {
            if (amount > 10)
            {
                tMP_InputField.text = 10.ToString();
                Debug.Log("Cant be larger than 10");
            }
            else if(amount<0)
            {
                tMP_InputField.text = 0.ToString();
                Debug.Log("Cant be less than 0");
            }
        }
        else
        {
            Debug.LogError("Could not parse string");
        }
    }

    public void OpenStageSelectionMenu()
    {
        mapSelectedPrevious = mapSelected;
        stageSelection.InitialiseWithPreviousMapSelected(mapSelectedPrevious);
    }

    public void EnableStartGame()
    {
        startGameButtonGO.gameObject.SetActive(true);
    }

    public void DisableStartGame()
    {
        startGameButtonGO.gameObject.SetActive(false);
    }

    public void ConfirmPrematchConditions()
    {
        int enemyCountAmount = 0;
        if (int.TryParse(enemyCountInputFieldText.text, out enemyCountAmount))
        {
            enemyCountInputFieldText.text = enemyCountAmount.ToString();
            enemyCount = enemyCountAmount;
        }
        else
        {
            Debug.LogError("Could not parse string");
        }

        enemyType = enemyType_tMP_Dropdown.value;
    }

    public void CancelPrematchConditions()
    {
        enemyCount = enemyCountPrevious;
        enemyType = enemyTypePrevious;
    }

    public void ConfirmStage()
    {
        mapSelected = stageSelection.GetSelectedBattleRoyaleMap();
    }

    public void CancelStage()
    {
        mapSelected = mapSelectedPrevious;
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