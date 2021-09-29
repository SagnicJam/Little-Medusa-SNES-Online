using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Match
{
    public int MatchID { get; set; }
    public int ProcessID { get; set; }
    public MatchConditionDto MatchConditionDto { get; set; }
    public Dictionary<string, PlayerInfoData> playerList { get; set; }
}
