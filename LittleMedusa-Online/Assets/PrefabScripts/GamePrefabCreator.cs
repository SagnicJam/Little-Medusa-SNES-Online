using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MedusaMultiplayer
{
    public class GamePrefabCreator : MonoBehaviour
    {
        public static GamePrefabCreator instance;

        [Header("Unit Template")]
        public GameObject rockFormation;
        public GameObject rockRemoval;
        public GameObject chestOpen;
        public GameObject bigExplosion;
        public GameObject smallExplosion;
        public GameObject lightning;
        public GameObject crackEffect;
        public GameObject dispersedBoulders;
        public GameObject dispersedFire;
        public GameObject leftFire;
        public GameObject rightFire;
        public GameObject upFire;
        public GameObject downFire;
        public GameObject waveBlock;
        public GameObject[] stalagmiteVar;
        public GameObject starTravelGO;
        public GameObject pegasusGO;
        public GameObject gameHUDGO;
        public GameObject[] monsterGOArr;
        public GameObject[] baseWorldArr;
        public GameObject[] projectileGOArr;
        public GameObject onHitMedusaVFX;
        public GameObject[] onHitEnemyVFX;
        public GameObject mirrorCrack;

        public GameObject[] stalagmiteShatter;

        private void Awake()
        {
            instance = this;
        }
    }
}