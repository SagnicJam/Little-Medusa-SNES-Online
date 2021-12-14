using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class ProjectileManager : MonoBehaviour
    {
        public int id;

        public bool isSpriteDependedonRotation;

        public bool isRotationControlledByTransform;

        public SpriteRenderer sp;

        public Sprite leftSprite;
        public Sprite rightSprite;
        public Sprite upSprite;
        public Sprite downSprite;

        public void OnInititialise(int id)
        {
            this.id = id;
        }

        public void SetPosition(Vector3 position)
        {
            //Debug.Log("position: "+position);
            transform.position = position;
        }

        public void SetFaceDirection(int faceDirection)
        {
            if (isRotationControlledByTransform)
            {
                switch ((FaceDirection)faceDirection)
                {
                    case FaceDirection.Down:
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case FaceDirection.Up:
                        transform.rotation = Quaternion.Euler(0, 0, 180);
                        break;
                    case FaceDirection.Left:
                        transform.rotation = Quaternion.Euler(0, 0, -90);
                        break;
                    case FaceDirection.Right:
                        transform.rotation = Quaternion.Euler(0, 0, 90);
                        break;
                }
            }
            else if (isSpriteDependedonRotation)
            {
                switch ((FaceDirection)faceDirection)
                {
                    case FaceDirection.Down:
                        sp.sprite = downSprite;
                        break;
                    case FaceDirection.Up:
                        sp.sprite = upSprite;
                        break;
                    case FaceDirection.Left:
                        sp.sprite = leftSprite;
                        break;
                    case FaceDirection.Right:
                        sp.sprite = rightSprite;
                        break;
                }
            }
        }
    }
}
