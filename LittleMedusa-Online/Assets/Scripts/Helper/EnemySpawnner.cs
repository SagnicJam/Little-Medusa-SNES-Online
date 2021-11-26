using System.Collections;
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

    int x;
    void SpawnNewEnemy()
    {
        currentEnemyCount++;
        //InstantiateEnemy(spawnIndexList[Random.Range(0, spawnIndexList.Count)]);
        x++;
        InstantiateEnemy(spawnIndexList[x% spawnIndexList.Count]);
    }

    void InstantiateEnemy(Vector3Int cellPos)
    {
        GameObject enemy = Instantiate(enemyPrefab[(int)monsterToSpawn]);
        Enemy actor = enemy.GetComponentInChildren<Enemy>();
        actor.isSpawnned = true;
        actor.transform.position = GridManager.instance.cellToworld(cellPos);
        actor.transform.rotation = Quaternion.identity;
        actor.currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actor.transform.position);
    }

    public void InstantiateEnemy(Vector3Int cellPos,int direction,int leaderId)
    {
        GameObject enemy = Instantiate(enemyPrefab[(int)monsterToSpawn]);
        Enemy actor = enemy.GetComponentInChildren<Enemy>();
        actor.leaderNetworkId = leaderId;
        actor.Facing = (FaceDirection)direction;
        actor.transform.position = GridManager.instance.cellToworld(cellPos);
        actor.transform.rotation = Quaternion.identity;
        actor.currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actor.transform.position);
    }

    public void InstantiatePetrifiedEnemy(Vector3Int cellPos, int direction,EnumData.MonsterBreed monsterBreed)
    {
        GameObject enemy = Instantiate(enemyPrefab[(int)monsterBreed]);
        Enemy actor = enemy.GetComponentInChildren<Enemy>();
        actor.Facing = (FaceDirection)direction;
        actor.transform.position = GridManager.instance.cellToworld(cellPos);
        actor.transform.rotation = Quaternion.identity;
        actor.currentMovePointCellPosition = GridManager.instance.grid.WorldToCell(actor.transform.position);
        actor.Petrify();
    }

    public int spawnTickRate;
    public int currentSpawnTick;

    private void FixedUpdate()
    {
        if(MultiplayerManager.instance.isServer&& startSpawnner&& spawnIndexList.Count>0)
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
