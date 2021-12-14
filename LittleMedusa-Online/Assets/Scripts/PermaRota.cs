using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class PermaRota : MonoBehaviour
    {
        public float speed;

        private void FixedUpdate()
        {
            transform.Rotate(transform.forward, speed * Time.fixedDeltaTime);
        }
    }
}