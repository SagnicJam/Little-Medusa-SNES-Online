using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyDataSender : MonoBehaviour
{
    public bool isInitialised;
    public EnumData.MonsterBreed monsterBreed;
    Enemy enemy;
    public void Initialise(Enemy enemy)
    {
        if (enemy is Cyclops)
        {
            monsterBreed = EnumData.MonsterBreed.Cyclops;
        }
        else if (enemy is Snake)
        {
            monsterBreed = EnumData.MonsterBreed.Snakes;
        }
        else if (enemy is Centaur)
        {
            monsterBreed = EnumData.MonsterBreed.Centaur;
        }
        else if (enemy is Minnataur)
        {
            monsterBreed = EnumData.MonsterBreed.Minotaur;
        }
        else if (enemy is ZeusHead)
        {
            monsterBreed = EnumData.MonsterBreed.ZeusHead;
        }
        else if (enemy is MirrorKnight)
        {
            monsterBreed = EnumData.MonsterBreed.MirrorKnight;
        }
        EnemyData enemyData = new EnemyData(enemy.ownerId,enemy.leaderNetworkId, (int)monsterBreed, enemy.frameLooper.spriteIndexToShowCache, (int)enemy.Facing, enemy.GetEnemyState(), enemy.actorTransform.position);
        ServerSideGameManager.enemiesDic.Add(enemy.ownerId, enemyData);
        this.enemy = enemy;
        isInitialised = true;
    }

    private void FixedUpdate()
    {
        if(isInitialised)
        {
            EnemyData enemyData;
            if (ServerSideGameManager.enemiesDic.TryGetValue(enemy.ownerId, out enemyData))
            {
                enemyData.animationIndexNumber = enemy.frameLooper.spriteIndexToShowCache;
                enemyData.faceDirection = (int)enemy.Facing;
                enemyData.enemyState = enemy.GetEnemyState();
                enemyData.enemyPosition = enemy.actorTransform.transform.position;


                ServerSideGameManager.enemiesDic.Remove(enemy.ownerId);
                ServerSideGameManager.enemiesDic.Add(enemy.ownerId, enemyData);
            }
            else
            {
                Debug.LogError("Doesnot contain the key to set projectile position for");
            }
        }
    }

    private void OnDestroy()
    {
        if (ServerSideGameManager.enemiesDic.ContainsKey(enemy.ownerId))
        {
            ServerSideGameManager.enemiesDic.Remove(enemy.ownerId);
        }
        else
        {
            Debug.LogError("Could not find owner id: " + enemy.ownerId);
        }
    }
}
