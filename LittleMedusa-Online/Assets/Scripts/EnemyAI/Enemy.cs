using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy stats")]
    public bool canBePetrified;
    public bool dieOnUnPetrification;
    public float walkSpeed;
    public MoveUseAnimationAction primaryMoveUseAction = new MoveUseAnimationAction();

    public List<Mapper> mapperList = new List<Mapper>();
    public static Dictionary<int, Enemy> enemies = new Dictionary<int, Enemy>();
    static int nextEnemyId = 1;

    public int id;

    public virtual void Awake()
    {
        id = nextEnemyId;
        nextEnemyId++;
        enemies.Add(id,this);
    }
}
