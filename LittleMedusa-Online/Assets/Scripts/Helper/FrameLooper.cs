using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class FrameLooper : MonoBehaviour
{
    public OnUsed<Actor> onMoveUseActionOver;

    public UnityEvent onPlayOneShotAnimation;
    public Sprite[] spriteArr;
    public SpriteRenderer spriteRenderer;

    public float animationDuration;
    public float temp;

    public bool IsLoopComplete;
    public bool playonAwakeWithLoop;
    public bool playonAwakeOneShot;
    public int spriteIndexToShowCache;

    private void Start()
    {
        if(playonAwakeOneShot)
        {
            PlayOneShotAnimation();
        }
    }

    private void FixedUpdate()
    {
        if(playonAwakeWithLoop)
        {
            UpdateAnimationFrame();
        }
    }

    public void SetStaticFrame(Sprite sp)
    {
        spriteRenderer.sprite = sp;
    }

    public void UpdateSpriteArr(Sprite[] spArr)
    {
        if(spArr.Length>0)
        {
            spriteIndexToShowCache = 0;
            spriteArr = spArr;
            spriteRenderer.sprite = spriteArr[spriteIndexToShowCache];
            temp = 0;
            IsLoopComplete = false;
        }
    }

    public bool playingOneShot;
    IEnumerator ie;
    public void PlayOneShotAnimation()
    {
        if (ie != null)
        {
            StopCoroutine(ie);
        }

        ie = PlayOneShot();
        StartCoroutine(ie);
    }

    public void StopOneShot()
    {
        StopCoroutine(ie);
    }

    IEnumerator PlayOneShot()
    {
        playingOneShot = true;
        spriteIndexToShowCache = 0;
        if (spriteArr.Length > 0)
        {
            while (!IsLoopComplete)
            {
                UpdateAnimationFrame();
                yield return new WaitForFixedUpdate();
            }
        }

        if (onPlayOneShotAnimation != null)
        {
            onPlayOneShotAnimation.Invoke();
        }
        playingOneShot = false;
        yield break;
    }

    public void UpdateAnimationFrame()
    {
        temp += Time.fixedDeltaTime;
        if (temp < animationDuration)
        {
            IsLoopComplete = false;
            spriteIndexToShowCache = Mathf.RoundToInt(Mathf.Lerp(0, spriteArr.Length - 1, temp / animationDuration));
            if (spriteIndexToShowCache < 0 || spriteIndexToShowCache >= spriteArr.Length)
            {
                Debug.LogError("instance id causing problem: " + gameObject.GetInstanceID() + "----> " + spriteIndexToShowCache);
                return;
            }
            spriteRenderer.sprite = spriteArr[spriteIndexToShowCache];
        }
        else
        {
            temp = 0;
            IsLoopComplete = true;
        }
    }

    public void DestroyObject()
    {
        Destroy(this.gameObject);
    }
}
