using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public abstract class Actions
    {
        public abstract void Initialise(Actor actorActingThisAction);
        public abstract bool Perform();
    }
}

