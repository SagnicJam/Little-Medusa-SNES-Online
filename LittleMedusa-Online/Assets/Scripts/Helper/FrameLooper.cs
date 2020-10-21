using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class FrameLooper : MonoBehaviour
{
    public UnityEvent onPlayOneShotAnimation;
    public Sprite[] spriteArr;
    public SpriteRenderer spriteRenderer;
    
    public float timeBetweenFrames;

    public float temp;
    public int spriteIndexToShow;

    public bool playonAwakeWithLoop;

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
            spriteIndexToShow = 0;
            spriteArr = spArr;
            spriteRenderer.sprite = spriteArr[spriteIndexToShow];
            temp = timeBetweenFrames;
        }
    }

    public bool playingOneShot;
    IEnumerator ie;
    public void PlayOneShotAnimation()
    {
        ie = PlayOneShot();
        StopCoroutine(ie);
        StartCoroutine(ie);
    }

    public void StopOneShot()
    {
        StopCoroutine(ie);
    }

    public bool isRepeatingLoop
    {
        get
        {
            return spriteIndexToShow + 1 == spriteArr.Length;
        }
    }

    IEnumerator PlayOneShot()
    {
        playingOneShot = true;
        spriteIndexToShow = 0;
        temp = timeBetweenFrames;
        if (spriteArr.Length > 0)
        {
            while (!isRepeatingLoop)
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
        if (temp >= 0f)
        {
            temp -= Time.fixedDeltaTime;
        }
        else
        {
            if (spriteArr.Length == 0)
            {
                return;
            }
            else
            {
                spriteIndexToShow = (spriteIndexToShow + 1) % spriteArr.Length;
                spriteRenderer.sprite = spriteArr[spriteIndexToShow];
                temp = timeBetweenFrames;
            }
        }
    }
}
