using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace MedusaMultiplayer
{
    public class MoveUseAnimationAction : Actions
    {
        public OnUsed<Actor> onMoveUseActionOver;
        public OnUsed<Actor> onMoveUseActionBegin;

        public float animationDuration;
        public float walkAnimationDuration;
        public bool canPerformMoveUseAnimations;
        public bool isCancellingMovePlayerInputDriven;

        Actor actorUsingMove;
        MoveAnimationSprites moveAnimationSprites;

        public override void Initialise(Actor actorActingThisAction)
        {
            actorUsingMove = actorActingThisAction;
        }

        public void SetAnimationSpeedAndSpritesOnUsage(float animationDuration, float walkAnimationDuration, MoveAnimationSprites moveAnimationSprites)
        {
            this.animationDuration = animationDuration;
            this.walkAnimationDuration = walkAnimationDuration;
            this.moveAnimationSprites = moveAnimationSprites;
        }


        public bool initialiseSprite;
        public bool isBeingUsed;

        public override bool Perform()
        {
            if (actorUsingMove.isPetrified)
            {
                return false;
            }
            if (!canPerformMoveUseAnimations)
            {
                return false;
            }
            if (!initialiseSprite)
            {
                if (onMoveUseActionBegin != null)
                {
                    onMoveUseActionBegin.Invoke(actorUsingMove);
                }
                //Debug.Log("initialiseSprite");
                actorUsingMove.frameLooper.animationDuration = animationDuration;
                switch (actorUsingMove.Facing)
                {
                    case FaceDirection.Down:
                        actorUsingMove.frameLooper.UpdateSpriteArr(moveAnimationSprites.downMove);
                        break;
                    case FaceDirection.Up:
                        actorUsingMove.frameLooper.UpdateSpriteArr(moveAnimationSprites.upMove);
                        break;
                    case FaceDirection.Left:
                        actorUsingMove.frameLooper.UpdateSpriteArr(moveAnimationSprites.leftMove);
                        break;
                    case FaceDirection.Right:
                        actorUsingMove.frameLooper.UpdateSpriteArr(moveAnimationSprites.rightMove);
                        break;
                }
                initialiseSprite = true;
                isBeingUsed = true;
            }
            else
            {
                actorUsingMove.frameLooper.UpdateAnimationFrame();
                if (!actorUsingMove.frameLooper.IsLoopComplete)
                {
                    return true;
                }
                else
                {
                    CancelMoveUsage();
                    if (onMoveUseActionOver != null)
                    {
                        onMoveUseActionOver.Invoke(actorUsingMove);
                    }
                    return false;
                }
            }
            return false;
        }

        public void CancelMoveUsage()
        {
            //Debug.Log("CancelMoveUsage");
            initialiseSprite = false;
            isBeingUsed = false;
            canPerformMoveUseAnimations = false;
            //Debug.Log("normalSpeed "+ normalSpeed);
            actorUsingMove.frameLooper.animationDuration = walkAnimationDuration;
            if (!isCancellingMovePlayerInputDriven)
            {
                actorUsingMove.UpdateBasicWalkingSprite();
            }
        }

        [Serializable]
        public class MoveAnimationSprites
        {
            public Sprite[] upMove;
            public Sprite[] downMove;
            public Sprite[] leftMove;
            public Sprite[] rightMove;

            public MoveAnimationSprites(Sprite[] upMove, Sprite[] downMove, Sprite[] leftMove, Sprite[] rightMove)
            {
                this.upMove = upMove;
                this.downMove = downMove;
                this.leftMove = leftMove;
                this.rightMove = rightMove;
            }
        }
    }
}


