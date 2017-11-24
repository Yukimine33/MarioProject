using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurtleEnemy : Enemy
{
    [SerializeField]
    private GameObject _body;
    [SerializeField]
    private GameObject _shell;
    [SerializeField]
    private Animator _bodyAnim;
    [SerializeField]
    private Animator _shellAnim;

    private float _normalSpeed = 1f; //正常速度
    private float _shellSpeed = 4f; //龟壳移动速度

    private float _checkDistance = 1f; //检测玩家和龟壳距离

    private bool _isCheck = false;
    private bool _isShellMoving = false;

    enum CurStage
    {
        body,
        shell,
    }

    enum ShellState
    {
        idle,
        move,
    }

    private CurStage curStage = CurStage.body;
    private ShellState shellState = ShellState.idle;

    void Start()
    {
        Init();
    }

    void Update()
    {
        stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        if (!_player.isDead && !isHit && !isDead)
        {
            Move();
        }

        if (_player.isDead)
        {
            anim.speed = 0;
        }

        if (isHit)
        {
            GetHit();
        }

        if(_isCheck)
        {
            _CheckDistance();
        }
    }

    protected override void Init()
    {
        speed = _normalSpeed;
        enemyLayer = LayerMask.NameToLayer("Enemy");
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();
        anim = _bodyAnim;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (curStage == CurStage.body)
            { _CheckPlayerPos(); }
            else if(curStage == CurStage.shell)
            { _CheckShellState(); }
        }
        else if (collision.CompareTag("Border") || collision.gameObject.layer == enemyLayer)
        {
            ChangeMoveDir();
        }
    }

    private void _CheckPlayerPos()
    {
        var playerPos = _player.checkPoint.position;
        var curPos = transform.position;

        if (playerPos.y - curPos.y > 0)
        {
            isHit = true;
            _player.Bounce();
        }
        else
        {
            _player.Die();
        }
    }

    protected override void GetHit()
    {
        _body.SetActive(false);
        _shell.SetActive(true);
        anim = _shellAnim;
        curStage = CurStage.shell;
        shellState = ShellState.idle;
        _isShellMoving = false;
        StartCoroutine("OnRecover");
    }

    IEnumerator OnRecover()
    {
        yield return new WaitForSeconds(3f);
        anim.SetTrigger("OnRecover");
        yield return new WaitForSeconds(2f);
        anim.SetBool("IsRecover", true);
        Recover();
    }

    void Recover()
    {
        _shell.SetActive(false);
        _body.SetActive(true);

        if (transform.localScale.x * dir.x == 1)
        {
            var scale = transform.localScale;
            scale.x *= -dir.x;
            transform.localScale = scale;
        }

        isHit = false;
        curStage = CurStage.body;

        speed = _normalSpeed;
        anim = _bodyAnim;

        StopCoroutine("OnRecover");
    }

    private void _CheckShellState()
    {
        if (shellState == ShellState.idle)
        {
            _EnterShellMove();
        }
        else if (shellState == ShellState.move)
        {
            _CheckShellMove();
        }
    }

    private void _EnterShellMove()
    {
        var temp = this.transform.position - _player.transform.position;

        if(temp.x > 0)
        {
            dir.x = 1;
        }
        else
        {
            dir.x = -1;
        }

        _isCheck = true;
        isHit = false;

        shellState = ShellState.move;

        speed = _shellSpeed;

        anim.Play("Shell", 0, 0);
        StopCoroutine("OnRecover");
    }

    private void _CheckShellMove()
    {
        if(_isShellMoving)
        {
            _CheckPlayerPos();
        }
        else
        {
            _EnterShellMove();
        }
    }

    private void _CheckDistance()
    {
        var temp = _player.transform.position - this.transform.position;

        if(temp.magnitude > _checkDistance)
        {
            _isShellMoving = true;
            _isCheck = false;
        }
    }
}
