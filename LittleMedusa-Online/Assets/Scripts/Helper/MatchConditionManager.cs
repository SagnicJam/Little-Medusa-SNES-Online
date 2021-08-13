using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MatchConditionManager : MonoBehaviour
{
    public static MatchConditionManager instance;

    private void Awake()
    {
        instance = this;
    }

    public int enemyCount;
    public int enemyType;

    public void SetCount(TMP_InputField tMP_InputField)
    {
        string inputString = tMP_InputField.text;
        int amount = 0;
        if (int.TryParse(inputString,out amount))
        {
            if(amount<=4)
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
}
