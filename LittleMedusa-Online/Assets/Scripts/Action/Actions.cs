using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Actions
{
    public abstract void Initialise(Actor actorActingThisAction);
    public abstract bool Perform();
}
