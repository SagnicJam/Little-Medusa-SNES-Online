using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientEnemyManager : MonoBehaviour
{
    [Header("Tweak Params")]
    public Color selfMinionColor;
    public Color enemyMinionColor;

    [Header("Scene references")]
    public SpriteRenderer spriteRenderer;

    [Header("Live Data")]
    public int id;
    public EnumData.EnemyState currentEnemyState;
    public FaceDirection currentFaceDirection;
    public EnemyData currentEnemyData;
    public EnumData.MonsterBreed enemyType;
    public EnemyDisplayData cyclopsEnemyDisplayData;
    public EnemyDisplayData snakeEnemyDisplayData;
    public EnemyDisplayData centaurEnemyDisplayData;
    public EnemyDisplayData zeusHeadEnemyDisplayData;
    public EnemyDisplayData minnataurEnemyDisplayData;
    public EnemyDisplayData mirrorKnightEnemyDisplayData;


    public Sprite[] currentAnimationSpriteGroup;


    public void SetEnemyColor(int leaderNetworkId)
    {
        if(leaderNetworkId == 0)
        {
            return;
        }
        if (leaderNetworkId == Client.instance.myID)
        {
            spriteRenderer.color = selfMinionColor;
        }
        else
        {
            spriteRenderer.color = enemyMinionColor;
        }
    }

    public void SetEnemyData(EnemyData enemyData)
    {
        //Debug.Log(JsonUtility.ToJson(enemyData));
        id = enemyData.uid;
        enemyType = (EnumData.MonsterBreed)enemyData.enemyType;
        SetState(enemyData.enemyState, enemyData.faceDirection);
        SetAnimationIndex(enemyData.animationIndexNumber);
        SetTransform(enemyData.enemyPosition);

        currentEnemyData = enemyData;
    }

    void SetAnimationIndex(int index)
    {
        if (currentAnimationSpriteGroup == null || currentAnimationSpriteGroup.Length == 0)
        {
            Debug.Log("sprite arr not initialised");
            return;
        }
        if (currentAnimationSpriteGroup.Length <= index || index < 0)
        {
            Debug.Log("index error " + index);
            return;
        }
        spriteRenderer.sprite = currentAnimationSpriteGroup[index];
    }

    void SetState(int enemyState,int faceDirection)
    {
        if(currentFaceDirection!=(FaceDirection)faceDirection|| currentEnemyState != (EnumData.EnemyState)enemyState)
        {
            UpdateSprite((EnumData.EnemyState)enemyState, (FaceDirection)faceDirection);
        }
        currentFaceDirection = (FaceDirection)faceDirection;
        currentEnemyState = (EnumData.EnemyState)enemyState;
    }

    void SetTransform(Vector3 position)
    {
        transform.position = position;
    }

    private void UpdateSprite(EnumData.EnemyState enemyState, FaceDirection faceDirection)
    {
        switch(enemyType)
        {
            case EnumData.MonsterBreed.Cyclops:
                UpdateEnemyAnimation(enemyState, faceDirection, cyclopsEnemyDisplayData);
                break;
            case EnumData.MonsterBreed.Centaur:
                UpdateEnemyAnimation(enemyState, faceDirection, centaurEnemyDisplayData);
                break;
            case EnumData.MonsterBreed.Snakes:
                UpdateEnemyAnimation(enemyState, faceDirection, snakeEnemyDisplayData);
                break;
            case EnumData.MonsterBreed.Minotaur:
                UpdateEnemyAnimation(enemyState, faceDirection, minnataurEnemyDisplayData);
                break;
            case EnumData.MonsterBreed.ZeusHead:
                UpdateEnemyAnimation(enemyState, faceDirection, zeusHeadEnemyDisplayData);
                break;
            case EnumData.MonsterBreed.MirrorKnight:
                UpdateEnemyAnimation(enemyState, faceDirection, mirrorKnightEnemyDisplayData);
                break;
        }
    }

    void UpdateEnemyAnimation(EnumData.EnemyState enemyState, FaceDirection faceDirection,EnemyDisplayData enemyDisplayData)
    {
        switch (enemyState)
        {
            case EnumData.EnemyState.Idle:
                break;
            case EnumData.EnemyState.Walking:
                UpdateDirectionSprite(enemyDisplayData.walkSprite, faceDirection);
                break;
            case EnumData.EnemyState.Petrified:
                currentAnimationSpriteGroup = enemyDisplayData.petrificationSprite;
                break;
            case EnumData.EnemyState.Pushed:
                break;
            case EnumData.EnemyState.PhysicsControlled:
                break;
            case EnumData.EnemyState.PrimaryMoveUse:
                UpdateDirectionSprite(enemyDisplayData.primaryMove, faceDirection);
                break;
            case EnumData.EnemyState.SecondaryMoveUse:
                UpdateDirectionSprite(enemyDisplayData.secondaryMove, faceDirection);
                break;
        }
    }

    void UpdateDirectionSprite(MoveUseAnimationAction.MoveAnimationSprites moveUseAnimationAction,FaceDirection faceDirection)
    {
        switch (faceDirection)
        {
            case FaceDirection.Up:
                currentAnimationSpriteGroup = moveUseAnimationAction.upMove;
                break;
            case FaceDirection.Down:
                currentAnimationSpriteGroup = moveUseAnimationAction.downMove;
                break;
            case FaceDirection.Left:
                currentAnimationSpriteGroup = moveUseAnimationAction.leftMove;
                break;
            case FaceDirection.Right:
                currentAnimationSpriteGroup = moveUseAnimationAction.rightMove;
                break;
        }
    }
}

[System.Serializable]
public struct EnemyDisplayData
{
    public Sprite[] petrificationSprite;
    public MoveUseAnimationAction.MoveAnimationSprites walkSprite;
    public MoveUseAnimationAction.MoveAnimationSprites primaryMove;
    public MoveUseAnimationAction.MoveAnimationSprites secondaryMove;
}

[System.Serializable]
public struct EnemyData
{
    public int uid;
    public int leaderNetworkId;
    public int enemyType;
    public int faceDirection;
    public int animationIndexNumber;
    public int enemyState;
    public Vector3 enemyPosition;

    public EnemyData(int uid,int leaderNetworkId, int enemyType,int animationIndexNumber, int faceDirection, int enemyState, Vector3 enemyPosition)
    {
        this.uid = uid;
        this.leaderNetworkId = leaderNetworkId;
        this.enemyType = enemyType;
        this.animationIndexNumber = animationIndexNumber;
        this.faceDirection = faceDirection;
        this.enemyState = enemyState;
        this.enemyPosition = enemyPosition;
    }
}