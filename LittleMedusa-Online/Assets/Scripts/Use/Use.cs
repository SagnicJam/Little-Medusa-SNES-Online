using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public abstract class Use
    {
        public Actor actorUsing;
        public OnUsed<Actor> onUseOver;
        public OnUsed<Actor> onUseBegin;

        public abstract void BeginToUse(Actor actorUsing, OnUsed<Actor> onUseBegin, OnUsed<Actor> onUseOver);

        public abstract void PerformUsage();
        public abstract void EndOfUse();

        public bool endTriggered;

    }
}