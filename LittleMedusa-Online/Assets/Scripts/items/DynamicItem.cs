﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace MedusaMultiplayer
{
    public delegate void OnUsed<T>(T obj);
    public class DynamicItem
    {
        public OnUsed<Actor> onUsed;

        public Attack ranged;
        public Use activate;
    }
}