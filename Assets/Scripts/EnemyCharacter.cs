using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacter : MonoBehaviour
{
    PlayerCharacter _player;

    float speed = 1f;

    Vector3 dir;
    Vector2 checkDir;

    float checkDistance = 0.05f;

    LayerMask borderLayer;
    LayerMask playerLayer;
    LayerMask enemyLayer;

    Animator enemyAnim;
    AnimatorStateInfo stateInfo;

    Animator shellAnim;

    float shellMoveSpeed = 5f;
    Vector3 shellMoveDir;

    [SerializeField]
    private Transform checkTran;

    public bool isDead = false;
    public bool isHit = false;

    bool isOnTrigger = false;
    bool isShellMove = false;
    bool isCheck = false;
    public bool isShellAttack = false;

    private GameObject _turtleBody;
    private GameObject _turtleShell;
    private Transform _dieTrigger;

    public enum EnemyType
    {
        Normal,
        Turtle,
        Shell,
    }

    public EnemyType charType;

    void Start ()
    {
        Init();
    }

    void Init()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();

        borderLayer = 1 << LayerMask.NameToLayer("Ground");
        playerLayer = 1 << LayerMask.NameToLayer("Player");
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");

        dir = new Vector3(-1, 0, 0);
        checkDir = new Vector2(-1, 0);

        if (this.gameObject.CompareTag("Normal"))
        {
            charType = EnemyType.Normal;
            enemyAnim = GetComponentInChildren<Animator>();
        }
        else if (this.gameObject.CompareTag("Special"))
        {
            charType = EnemyType.Turtle;
            enemyAnim = transform.Find("Body").GetComponent<Animator>();
            _turtleBody = transform.Find("Body").gameObject;
            _turtleShell = transform.Find("Shell").gameObject;
            _dieTrigger = transform.Find("DieTrigger");
            shellAnim = _turtleShell.GetComponent<Animator>();
        }
    }
	
	void Update ()
    {
        stateInfo = enemyAnim.GetCurrentAnimatorStateInfo(0);

        if (!_player.isDead && !isHit && !isDead)
        {
            CheckBorder();
            CheckCharacter();
            Move();
        }

        if(_player.isDead)
        {
            enemyAnim.speed = 0;
        }

        if(isHit && charType == EnemyType.Normal)
        {
            NormalEnemyHit();
        }

       

        if(charType == EnemyType.Shell)
        {
            if (!isOnTrigger)
            {
                CheckTrigger();
            }

            if(isShellMove && !_player.isDead)
            {
                ShellMove();
                CheckBorder();
            }

            if(isCheck)
            {
                CheckDistance();
            }
        }

        if (isShellAttack)
        {
            CheckAttack();
        }

        if (isDead)
        {
            Die();
        }
    }

    void Move()
    {
        this.transform.position += dir * Time.deltaTime * speed;
    }

    /// <summary>
    /// 若碰到障碍物则折返
    /// </summary>
    public void CheckBorder()
    {
        Vector2 checkPos = checkTran.position;
        RaycastHit2D borderHit = Physics2D.Raycast(checkPos, checkDir, checkDistance, borderLayer.value);
        

        if (borderHit.collider != null)
        {
            ChangeMoveDir();
        }
    }

    void CheckCharacter()
    {
        Vector2 checkPos = checkTran.position;
        RaycastHit2D characterHit = Physics2D.Raycast(checkPos, checkDir, checkDistance, enemyLayer.value);


        if (characterHit.collider != null)
        {
            if (characterHit.collider.CompareTag("Normal") || characterHit.collider.CompareTag("Special"))
            {
                characterHit.collider.gameObject.GetComponentInParent<EnemyCharacter>().ChangeMoveDir();
            }

            if (charType != EnemyType.Shell)
            {
                ChangeMoveDir();
            }
        }
    }

    void NormalEnemyHit()
    {
        enemyAnim.SetTrigger("Hit");
        CloseCollidersInChild(this.transform);
        if (stateInfo.IsName("Hit") && stateInfo.normalizedTime >= 1f)
        {
            this.gameObject.SetActive(false);
        }
    }

    void Reverse()
    {
        var scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void ChangeMoveDir()
    {
        
        dir.x *= -1;
        checkDir.x *= -1;
        Reverse();
        
        if (charType == EnemyType.Shell)
        { shellMoveDir.x *= -1; }
    }

    public void GetHit(int rStage)
    {
        if(charType == EnemyType.Turtle)
        {
            _turtleBody.SetActive(false);
            _turtleShell.SetActive(true);
            isHit = true;
            _dieTrigger.gameObject.SetActive(false);
            charType = EnemyType.Shell;
        }
        else if(charType == EnemyType.Shell)
        {
            isShellMove = false;
            isShellAttack = false;
            isOnTrigger = false;
            _player.InitCount();
        }

        StartCoroutine("OnRecover");
    }

    IEnumerator OnRecover()
    {
        yield return new WaitForSeconds(3f);
        shellAnim.SetTrigger("OnRecover");
        yield return new WaitForSeconds(2f);
        shellAnim.SetBool("IsRecover", true);
        Recover();
    }

    void Recover()
    {
        _turtleShell.SetActive(false);
        _turtleBody.SetActive(true);

        Debug.Log("dir.x:" + dir.x + " transform.localScale.x:" + transform.localScale.x);
        if(transform.localScale.x * dir.x == 1)
        {
            var scale = transform.localScale;
            scale.x *= -dir.x;
            transform.localScale = scale;
        }

        isHit = false;
        isOnTrigger = false;
        _dieTrigger.gameObject.SetActive(true);
        charType = EnemyType.Turtle;
        _player.InitCount();
    }

    void CheckTrigger()
    {
        Vector2 checkPos = transform.position;
        Vector2 playerPos = _player.transform.position;
        var hit = Physics2D.OverlapCircle(checkPos, 0.1f, playerLayer.value);

        if(hit != null)
        {
            isShellMove = true;
            isOnTrigger = true;
            isCheck = true;
            isShellAttack = true;

            var tempDir = checkPos - playerPos;
            if(tempDir.x > 0)
            {
                shellMoveDir = new Vector3(1, 0, 0);
                checkDir = new Vector2(1, 0);
            }
            else
            {
                shellMoveDir = new Vector3(-1, 0, 0);
                checkDir = new Vector2(-1, 0);
            }

            if (checkDir.x * dir.x == -1)
            {
                Reverse();
            }

            shellAnim.Play("Shell", 0, 0);
            StopCoroutine("OnRecover");
        }
    }

    void ShellMove()
    {
        dir.x = shellMoveDir.x;
        transform.position += shellMoveDir * Time.deltaTime * shellMoveSpeed;
    }

    void CheckDistance()
    {
        Vector2 checkPos = transform.position;
        Vector2 playerPos = _player.transform.position;
        var distance = (checkPos - playerPos).magnitude;
        if(distance > 1f)
        {
            _dieTrigger.gameObject.SetActive(true);
            _player.InitCount();
            isCheck = false;
        }
    }

    void CheckAttack()
    {
        Vector2 checkPos = checkTran.position;
        RaycastHit2D hit = Physics2D.Raycast(checkPos, checkDir, 0.08f, enemyLayer.value);

        if(hit.collider != null)
        {
            ShellAttack(hit.collider);
        }
    }

    void ShellAttack(Collider2D rCollider)
    {
        if (rCollider.CompareTag("Normal") || rCollider.CompareTag("Special"))
        { rCollider.gameObject.GetComponentInParent<EnemyCharacter>().isDead = true; }
    }

    void Die()
    {
        CloseCollidersInChild(this.transform);
        enemyAnim.SetTrigger("Die");
        if(stateInfo.IsName("Die") && stateInfo.normalizedTime >= 0.9f)
        {
            Destroy(this.gameObject);
        }
    }

    void CloseCollidersInChild(Transform rTran)
    {
        var tempTrans = rTran.GetComponentsInChildren<BoxCollider2D>();
        foreach(var child in tempTrans)
        {
            child.enabled = false;
        }
    }
}
