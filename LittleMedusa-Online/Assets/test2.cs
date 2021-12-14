using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace MedusaMultiplayer
{
    public class test2 : MonoBehaviour
    {
        public Rigidbody rb;
        public test test;
        public float x;
        PhysicsScene physicsScene;


        private void Start()
        {
            Scene scene = SceneManager.GetActiveScene();
            physicsScene = scene.GetPhysicsScene();
            Physics.autoSimulation = false;

            for (int i = 0; i < 100; i++)
            {
                //transform.Translate((Vector3.forward * Time.fixedDeltaTime * 4));
                rb.velocity = Vector3.forward * 4;
                Debug.Log("wuhh! transform " + rb.transform.position);
                Debug.Log("wuhh! physics " + rb.position);
                //Physics.SyncTransforms();
                physicsScene.Simulate(Time.fixedDeltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log("other 2: " + other.name);
            //test.IsKepe = true;
        }
    }
}