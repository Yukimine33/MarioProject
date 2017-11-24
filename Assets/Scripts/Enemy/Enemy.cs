using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected PlayerCharacter _player;
    protected Rigidbody2D enemyRig;

    protected bool isHit = false;
    protected bool isDead = false;

    protected Animator anim;
    protected AnimatorStateInfo stateInfo;

    protected LayerMask enemyLayer;

    protected float speed = 1f;
    protected Vector3 dir = new Vector3(-1, 0, 0);

    protected virtual void Init() { }

    protected void Move()
    {
        transform.position += dir * Time.deltaTime * speed;
    }

    protected void Reverse()
    {
        var scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    protected void ChangeMoveDir()
    {
        dir.x *= -1;
        Reverse();
    }

    protected void CloseCollidersInChild(Transform rTran)
    {
        var tempTrans = rTran.GetComponentsInChildren<BoxCollider2D>();
        foreach (var child in tempTrans)
        {
            child.enabled = false;
        }
    }

    protected virtual void GetHit() { }
}
