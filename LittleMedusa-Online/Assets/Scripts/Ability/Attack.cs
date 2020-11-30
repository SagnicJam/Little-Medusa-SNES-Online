using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class Attack
{
    public int attackingActorOwnerId;
    public int damage;
    public EnumData.AttackTypes attackType;
    public EnumData.Projectiles projectiles;
    public FaceDirection actorFacingWhenFired;

    public bool isInitialised;

    public Attack(int damage,int attackingActorOwnerId, EnumData.AttackTypes attackType,EnumData.Projectiles projectiles)
    {
        isInitialised = true;
        this.damage = damage;
        this.attackType = attackType;
        this.projectiles = projectiles;
        this.attackingActorOwnerId = attackingActorOwnerId;
    }

    public void OnHit(Actor actorHit)
    {
        if(!isInitialised)
        {
            return;
        }
        if(actorHit.isInvincible)
        {
            return;
        }
        //if(actorHit is Boss&&attackType!=EnumData.AttackTypes.TouchAttack)
        //{
        //    return;
        //}

        if (attackType == EnumData.AttackTypes.ProjectileAttack)
        {
            switch(projectiles)
            {
                case EnumData.Projectiles.EyeLaser:
                    actorHit.PetrificationCommandRegister(attackingActorOwnerId);
                    break;
                case EnumData.Projectiles.FlamePillar:
                    actorHit.TakeDamage(damage);
                    break;
            }
        }
        //if(actorHit is Boss bossHit)
        //{
        //    bossHit.gethitShakeAction.shouldShake = true;
        //    if (AudioManager.instance != null)
        //        AudioManager.instance.PlayClip(AudioManager.instance.onBossHurt);
        //}


        //if (actorHit.health <= 0f)
        //{
        //if (actorHit is Monster)
        //{
        //    if (projectiles == EnumData.Projectiles.EyeLaser)
        //    {
        //        actorHit.Petrify();
        //    }
        //    if (projectiles == EnumData.Projectiles.Arrow || projectiles == EnumData.Projectiles.BoulderProjectile ||
        //        projectiles == EnumData.Projectiles.CardinalProjectile || projectiles == EnumData.Projectiles.FioraSpark || projectiles == EnumData.Projectiles.FireBall || projectiles == EnumData.Projectiles.FireTitanJuggler ||
        //        projectiles == EnumData.Projectiles.HurricaneWind || projectiles == EnumData.Projectiles.Water)
        //    {
        //        actorHit.Die();
        //    }
        //}

        //if (actorHit is Boss)
        //{
        //    if (!actorHit.isDead)
        //    {
        //        actorHit.Die();
        //    }
        //}
        //}
        ////if (attackType == EnumData.AttackTypes.KillAttack)
        ////{
        ////    actorHit.health = 0f;
        ////    if (!actorHit.isDead)
        ////    {
        ////        actorHit.Die();
        ////    }
        ////}
        ////else if(attackType == EnumData.AttackTypes.PetrificationAttack)
        ////{
        ////    actorHit.health = 0f;
        ////    if(actorHit is Monster)
        ////    {
        ////        if (!actorHit.isPetrified)
        ////        {
        ////            actorHit.Petrify();
        ////        }
        ////    }
        ////    else
        ////    {
        ////        if (!actorHit.isDead)
        ////            actorHit.Die();
        ////    }
        ////}
        ////else
        ////{
        ////    Debug.LogError("Invalid attack type");
        ////}
    }
}
