using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class StaticAnimatingTileManager : MonoBehaviour
    {
        public int id;
        public Sprite[] spArr;

        public SpriteRenderer spRenderer;

        public void SetID(int id)
        {
            this.id = id;
        }

        public void SetPosition(Vector3Int v)
        {
            transform.position = GridManager.instance.cellToworld(v);
        }

        public void SetSprite(int index)
        {
            spRenderer.sprite = spArr[index];
        }
    }
}
