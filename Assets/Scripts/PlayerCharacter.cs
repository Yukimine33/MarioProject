using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D rig2d;
    [SerializeField]
    Animator anim;
    [SerializeField]
    Transform checkPoint;

    float curSpeed = 3f;
    float maxSpeed = 5f;
    float jumpHeight = 350f;
    bool isFacingRight = true;
    bool isGrounded = true;

    float checkDistance = 0.05f;

    int hitCount = 0;
    public bool isDead = false;

    LayerMask groundLayer;
    LayerMask enemyLayer;

    Animator playerAnim;
    AnimatorStateInfo stateInfo;

    void Start ()
    {
        Init();

    }

    void Update()
    {
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (isDead && stateInfo.IsName("Die"))
        {
            return;
        }

        var h = Input.GetAxis("Horizontal");

        if (!isDead)
        { Move(h); }

        CheckIsGrounded();

        if (h > 0 && !isFacingRight)
        {
            Reverse();
        }
        else if (h < 0 && isFacingRight)
        {
            Reverse();
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
        }

        if(!isDead)
        {
            CheckHit();
        }
    }

    void Init()
    {
        rig2d = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        checkPoint = transform.Find("GroundCheckPoint");
        playerAnim = GetComponent<Animator>();

        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
    }

    /// <summary>
    /// 反转角色
    /// </summary>
    void Reverse()
    {
        if (isGrounded)
        {
            isFacingRight = !isFacingRight;
            var scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    void Move(float dic)
    {
        rig2d.velocity = new Vector2(dic * curSpeed, rig2d.velocity.y);
        anim.SetFloat("Speed", Mathf.Abs(dic * curSpeed));
        anim.SetFloat("MoveSpeed", curSpeed);
    }

    void Jump()
    {
        rig2d.AddForce(new Vector2(0, jumpHeight));
    }

    void CheckIsGrounded()
    {
        Vector2 check = checkPoint.position;
        RaycastHit2D hit = Physics2D.Raycast(check, Vector2.down, checkDistance, groundLayer.value);

        if (hit.collider != null)
        {
            anim.SetBool("IsGrounded", true);
            isGrounded = true;
        }
        else
        {
            anim.SetBool("IsGrounded", false);
            isGrounded = false;
        }
    }

    void CheckHit()
    {
        var check = checkPoint.position;
        var hit = Physics2D.OverlapCircle(check, 0.07f, enemyLayer.value);

        if (hit != null)
        {
            if (hit.CompareTag("Normal")) //若踩中普通怪物，则给予玩家一个反弹力，并触发怪物的死亡效果
            {
                Debug.Log("Hit Normal!");
                rig2d.velocity = new Vector2(rig2d.velocity.x, 5f);
                hit.GetComponentInParent<EnemyCharacter>().isHit = true;
            }
            else if (hit.CompareTag("Special")) //若踩中特殊怪物（乌龟），则在敌人相关代码中做对应变化
            {
                hitCount += 1;
                if (hitCount == 1)
                {
                    rig2d.velocity = new Vector2(rig2d.velocity.x, 5f);
                    hit.GetComponentInParent<EnemyCharacter>().GetHit(1);
                }
            }
        }
    }

    public void InitCount()
    {
        hitCount = 0;
    }

    public void Die()
    {
        Debug.Log("Player Die!");
        isDead = true;
        playerAnim.SetTrigger("Die");
        rig2d.velocity = new Vector2(0, 0);
    }
}
