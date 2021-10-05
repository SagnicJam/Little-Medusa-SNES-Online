﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnner : MonoBehaviour
{
    public List<GameObject> enemyPrefab;
    public EnumData.MonsterBreed monsterToSpawn;

    public List<Vector3Int> spawnIndexList = new List<Vector3Int>();

    public int totalEnemyToSpawn;

    public int currentEnemyCount;

    public bool liveEnemy;

    public bool startSpawnner;

    private void Start()
    {
        spawnIndexList = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.SpawnJar);
    }

    public void InitialiseSpawnner(int enemyType,int enemyCount)
    {
        startSpawnner = true;
        //Debug.LogError("enemy: "+enemyType);
        monsterToSpawn = (EnumData.MonsterBreed)enemyType;
        totalEnemyToSpawn = enemyCount;
    }

    void SpawnNewEnemy()
    {
        currentEnemyCount++;
        GameObject enemy = Instantiate(enemyPrefab[(int)monsterToSpawn]);
        Actor actor = enemy.GetComponentInChildren<Actor>();
        actor.transform.position = GridManager.instance.cellToworld(spawnIndexList[Random.Range(0,spawnIndexList.Count)]);
        actor.transform.rotation = Quaternion.identity;
        actor.currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actor.transform.position);
    }

    public int spawnTickRate;
    public int currentSpawnTick;

    private void FixedUpdate()
    {
        if(MultiplayerManager.instance.isServer&& startSpawnner)
        {
            liveEnemy = currentEnemyCount < totalEnemyToSpawn;
            if (liveEnemy)
            {
                if (spawnTickRate <= currentSpawnTick)
                {
                    SpawnNewEnemy();
                    currentSpawnTick = 0;
                }
                else
                {
                    currentSpawnTick ++;
                }
            }
        }
        
    }
}