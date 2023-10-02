using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BossState
{
    Ready,
    Start,
    Idle,
    L_Walk,   
    L_Jump,    
    L_Run,
    R_Walk,
    R_Jump,
    R_Run,
    Throw,
    Hit,
    Death,
}

public class BossController : MonoBehaviour
{
    public AudioClip HitClip;

    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer renderer;

    //-------Monster Info
    public static BossState m_BossState = BossState.Ready;

    public float m_MaxHp = 100;
    public float m_CurHp = 100;

    float MovePower = 1;
    
    //-------Monster Info

    int movementFlag = 0;
    int a_Count = 0;
    float JumpDelay = 0;

    //총알 관련 변수
    public GameObject m_RockObj =null;
    private Vector3 a_CacEndVec;

    Vector3 BossPos;
    float AtteckTick = 0;
    //총알 관련 변수

    // Start is called before the first frame update
    void Start()
    {
        m_MaxHp = 100;
        m_CurHp = 100;

        m_BossState = BossState.Idle;

        rigid = this.gameObject.GetComponent<Rigidbody2D>();
        anim = this.gameObject.GetComponent<Animator>();
        renderer = gameObject.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (m_BossState == BossState.Start)
        {
            if (this.transform.position.x > 44)
            {
                this.transform.Translate(Vector3.left * Time.deltaTime);
                anim.SetBool("IsWalk", true);
            }
            else
            {
                StartCoroutine("ChangeMovement");

                GameManager.m_CamMove = false;
                GameManager.m_GameState = GameState.Play;
                m_BossState = BossState.Idle;
                anim.SetBool("IsWalk", false);
            }
        }

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Boss_Throw") == true)   //총알발사
        {
            if (AtteckTick < 0.8f)
                AtteckTick += Time.deltaTime;
            if(AtteckTick > 0.8f)
            {
                Atteck();
                AtteckTick = 0;
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
        Direction();

        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
            anim.SetBool("IsJump", false);

        if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f && anim.GetCurrentAnimatorStateInfo(0).IsName("Boss_Death") == true)
        {
            
            Destroy(gameObject);
        }

    }

    void Move()
    {
        Vector3 moveVelocity = Vector3.zero;

        if (movementFlag == 3)  //왼쪽
        {
            moveVelocity = Vector3.left;
            anim.SetBool("IsWalk", true);
        }
        else if (movementFlag == 6) //오른쪾
        {
            moveVelocity = Vector3.right;
            anim.SetBool("IsWalk", true);
        }
        else if (movementFlag == 4 || movementFlag == 7) //Jump
        {  
            if (a_Count != movementFlag)
            {
                if (JumpDelay < 0.5f)
                    JumpDelay += Time.deltaTime;

                if (JumpDelay > 0.5f)
                {
                    JumpDelay = 0f;
                    this.rigid.AddForce(Vector2.up * 30f, ForceMode2D.Impulse);

                    if (movementFlag == 4)
                    this.rigid.AddForce(Vector2.left * 10f, ForceMode2D.Impulse);
                    else if (movementFlag == 7)
                        this.rigid.AddForce(Vector2.right * 10f, ForceMode2D.Impulse);

                    a_Count = movementFlag;
                }

            }            
        }
        else if (movementFlag == 5 || movementFlag == 8)    //Run
        {
            if (movementFlag == 5)
                moveVelocity = Vector3.left * 3;
            else
                moveVelocity = Vector3.right * 3;
        }        
        if(movementFlag != 9)
        transform.position += moveVelocity * MovePower * Time.deltaTime;

    }

    IEnumerator ChangeMovement()
    {
        anim.SetBool("IsWalk", false);
        anim.SetBool("IsJump", false);
        anim.SetBool("IsRun", false);
        anim.SetBool("IsThrow", false);

        movementFlag = Random.Range(2, 10);  //2~9

        a_Count = 0;

        //애니메이션 움직임
        if (movementFlag == 2)
        {
            anim.SetBool("IsWalk", false);
        }
        else if (movementFlag == 3 || movementFlag == 6)
        {
            anim.SetBool("IsWalk", true);
        }
        else if (movementFlag == 4 || movementFlag == 7)
        {
            anim.SetBool("IsJump", true);
        }
        else if (movementFlag == 5 || movementFlag == 8)
        {
            anim.SetBool("IsRun", true);
        }
        else if (movementFlag == 9)
        {
            anim.SetBool("IsThrow", true);
        }

        yield return new WaitForSeconds(3f);

        StartCoroutine("ChangeMovement");//다시호출
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(renderer.flipX ==true)   //왼쪽을 보고있으면
            movementFlag = Random.Range(6, 9); //오른쪽 모션 랜덤
        else
            movementFlag = Random.Range(2, 6); //왼쪽 모션 랜덤
    }

    public void TakeDamage(float a_Val)
    {
        EffectMgr.Instance.PlayEffect(HitClip.name);

        if (m_CurHp > 0)
        {
            m_CurHp -= a_Val;
            if (m_CurHp <= 0)
            {
                m_CurHp = 0;
                Die();
            }
            else
            anim.SetTrigger("Hit");
        }

        if (m_CurHp <= 0)
        {
            m_CurHp = 0;
            Die();
        }
    }

    void Direction()
    {
        if (movementFlag >= 3 && movementFlag <= 5)
            renderer.flipX = true;
        else if (movementFlag >= 6 && movementFlag <= 8)
            renderer.flipX = false;
    }

    public void Atteck()
    {
        BossPos = transform.localScale;
        GameObject newObj = (GameObject)Instantiate(m_RockObj);    //오브젝트의 클론 생성 함수

        if (renderer.flipX != true)
            a_CacEndVec = Vector3.right;
        else
            a_CacEndVec = Vector3.left;

        RockController a_RockSC = newObj.GetComponent<RockController>();

        a_RockSC.BulletSpawn(this.transform, a_CacEndVec);
    }

    void Die()
    {
        m_BossState = BossState.Death;
        GameManager.m_GameState = GameState.End;
        StopCoroutine("ChangeMovement");
        anim.SetBool("IsWalk", false);
        anim.SetBool("IsJump", false);
        anim.SetBool("IsRun", false);
        anim.SetBool("IsThrow", false);

        anim.SetTrigger("Die");
    }
}
