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

    public bool matchStarted;

    private void Start()
    {
        spawnIndexList = GridManager.instance.GetAllPositionForTileMap(EnumData.TileType.SpawnJar);
        if(MultiplayerManager.instance.isDebug)
        {
            InitialiseSpawnner((int)monsterToSpawn, totalEnemyToSpawn);
        }
    }

    public void InitialiseSpawnner(int enemyType,int enemyCount)
    {
        matchStarted = true;
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
        if(MultiplayerManager.instance.isServer&& matchStarted)
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
