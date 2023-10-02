using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum PlayerState
{
    Play,
    Die,
}

public class PlayerController : MonoBehaviour
{
    GameManager m_GameMgr;
    PlayerState m_Playerstate = PlayerState.Play;

    public float MovePower = 1f;
    public float MaxSpeed = 3f;
    public float JumpPower = 10f;

    //-------총알 발사 관련 변수
    private Vector3 a_CurPos;
    private Vector3 a_CacEndVec;
    public static Vector3 PlayerPos;
    float AtteckTime = 0f;

    public GameObject m_BulletObj = null;   //총알

    //-------총알 발사 관련 변수

    Rigidbody2D rigid;
    SpriteRenderer renderer;
    Animator anim;

    Vector3 Movement;

    int JumpCount = 0;
    float DoubleTick = 0.5f;
    bool Double = false;

    public static bool isUnBeatTime = false;//무적상태
    public Image[] Heart = new Image[5];    //체력

    GameObject ItemCheck = null;
    public static bool SpringAni = false;

    bool swimming = false;

    public static bool m_RepeatBlock = false;

    [Header("Audio")]
    public AudioClip ShootAudio;
    public AudioClip JumpAudio;
    public AudioClip ItemAudio;
    public AudioClip HitAudio;

    // Start is called before the first frame update
    void Start()
    {
        m_GameMgr = FindObjectOfType<GameManager>();

        rigid = gameObject.GetComponent<Rigidbody2D>();
        renderer = gameObject.GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_Playerstate == PlayerState.Die)
            if (Heart[0].fillAmount <= 0)
            {
                Die();
            }
        
        if (m_Playerstate != PlayerState.Play)
            return;

        Stop();

        if (GameManager.m_GameState == GameState.Play)
        {
            Direction();
            Jump();
            Animation();
            Goto_Stage2();
            Goto_Stage3();

            if (GameManager.m_BossStage == true && GameManager.m_Stage == Stage.Stage4)
                Goto_Boss();
        }
        
    }

    private void FixedUpdate()
    {
        if (m_Playerstate != PlayerState.Play)
            return;

        if (GameManager.m_GameState == GameState.Play)
            Move();
        Randing();
        Spring();
    }

    void Move()
    { 
        float h = Input.GetAxis("Horizontal");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);
        if (rigid.velocity.x > MaxSpeed)
            rigid.velocity = new Vector2(MaxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < MaxSpeed * (-1))
            rigid.velocity = new Vector2(MaxSpeed * (-1), rigid.velocity.y);
    }

    void Stop()
    {
        if(Input.GetButtonUp("Horizontal"))
        {            
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.3f, rigid.velocity.y);
        }
    }

    void Direction()
    {
        if (Input.GetButtonDown("Horizontal"))
            renderer.flipX = Input.GetAxisRaw("Horizontal") == -1;
    }

    void Animation()
    {
        if (Mathf.Abs(rigid.velocity.normalized.x) < 0.3f)
            anim.SetBool("IsMoving", false);
        else
            anim.SetBool("IsMoving", true);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (JumpCount > 1)
                return;

            EffectMgr.Instance.PlayEffect(JumpAudio.name);
            
            JumpCount++;

            if (swimming && JumpCount >=2)
                JumpCount = 0;

            if (JumpCount >= 2 && swimming == false)
            {
                Double = true;
                anim.SetBool("DoubleJump", true);
            }

            
            rigid.AddForce(Vector2.up * JumpPower, ForceMode2D.Impulse);
            anim.SetBool("IsJumping", true);
        }

        if(Double == true)
        {
            if (DoubleTick > 0)
                DoubleTick -= Time.deltaTime;
            if(DoubleTick < 0)
            {
                DoubleTick = 0.5f;
                Double = false;
                anim.SetBool("DoubleJump", false);
            }

        }
    }

    void Randing()
    {
        if (rigid.velocity.y < 0)
        {
            Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Platfrom"));

            if (rayHit.collider != null)
            {
                if (rayHit.distance < 0.65f)
                {
                    anim.SetBool("IsJumping", false);
                    JumpCount = 0;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (m_Playerstate != PlayerState.Play)
            return;

        if (collision.gameObject.name.Contains("Monster") == true )
        {
            if(!isUnBeatTime)
            TakeDamage(collision);
        }

        if(collision.gameObject.tag == "Item" )
        {
            GetItem(collision);
        }      
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (m_Playerstate != PlayerState.Play)
            return;

        if (collision.gameObject.name == "Block")
        {
            m_RepeatBlock = true;
        }

        if(collision.gameObject.tag == "Trap")
        {
            if (!isUnBeatTime)
                TakeDamage(collision);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (m_Playerstate != PlayerState.Play)
            return;

        if (collision.gameObject.name == "Block")
        {
            m_RepeatBlock = false;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_Playerstate != PlayerState.Play)
            return;

        if (collision.gameObject.name.Contains("WaterGroup"))
        {
            swimming = true;
            rigid.gravityScale = 1f;
            JumpCount = 0;
            anim.SetBool("IsJumping", true);
        }
        if (collision.gameObject.name == "NextStage")
            Goto_Stage4();

        if (collision.gameObject.tag == "Monster")
        {
            if (!isUnBeatTime)
                TakeDamage();
        }
    }

    float delayTick = 0;
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (m_Playerstate != PlayerState.Play)
            return;

        if (collision.gameObject.name.Contains("LavaGroup"))
        {
            if (!isUnBeatTime)
                TakeDamage();
        }

        if (collision.gameObject.name == "Potal")
        {
            if (m_GameMgr.m_PotalLight.intensity < 1)
                m_GameMgr.m_PotalLight.intensity += Time.deltaTime;

            if (delayTick < 3f)
                delayTick += Time.deltaTime;

            if (delayTick > 0.5f)
            {
                if(m_GameMgr.m_PlayerLight.intensity >0.5f)
                m_GameMgr.m_PlayerLight.intensity -= Time.deltaTime;
            }

            if (delayTick >= 3f)
                GameManager.m_BossStage = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (m_Playerstate != PlayerState.Play)
            return;

        if (collision.gameObject.name.Contains("WaterGroup"))
        {
            rigid.gravityScale = 2f;
            swimming = false;
            anim.SetBool("IsJumping", false);
        }
        
    }

    public void Atteck()
    {
        if (GameManager.m_GameState == GameState.CamMove)
            return;
        
        PlayerPos = transform.localScale;
        GameObject newObj = (GameObject)Instantiate(m_BulletObj);    //오브젝트의 클론 생성 함수

        EffectMgr.Instance.PlayEffect(ShootAudio.name);


        if (renderer.flipX != true)
            a_CacEndVec = Vector3.right;
        else
            a_CacEndVec = Vector3.left;

        BulletController a_BulletSC = newObj.GetComponent<BulletController>();

        a_BulletSC.BulletSpawn(this.transform, a_CacEndVec);        
    }

    IEnumerator UnBeatTime()//무적 코루틴
    {

        int countTime = 0;

        while (countTime < 5)
        {     
            yield return new WaitForSeconds(0.2f);
            countTime++;
        }
        isUnBeatTime = false;
        yield return null;
    }

    public void TakeDamage(Collision2D collision = null)
    {
        if (GameManager.m_GameState == GameState.End)
            return;

        if (BossController.m_BossState == BossState.Death)
            return;

        if(Heart[0].fillAmount <= 0)
        {
            Die();
        }

        EffectMgr.Instance.PlayEffect(HitAudio.name);

        //밀려남 처리
        Vector2 damageVelocity = Vector2.zero;
        if (collision != null)
        {
            if (collision.gameObject.transform.position.x > transform.position.x)   //몬스터가 오른쪽에 있을경우
                damageVelocity = new Vector2(-7f, 4f);
            else
                damageVelocity = new Vector2(7f, 4f);
            rigid.AddForce(damageVelocity, ForceMode2D.Impulse);
        }
        else
        {
            int RandVec = Random.Range(-1, 2);
            damageVelocity = new Vector2(RandVec*7f, 4f);
            rigid.AddForce(damageVelocity, ForceMode2D.Impulse);
        }
        //밀려남 처리

        if (Heart[0].fillAmount != 0.5f)
        {
            isUnBeatTime = true;            
            anim.SetTrigger("Hit");

            StartCoroutine("UnBeatTime");
        }
        
        for (int i = Heart.Length-1; i >= 0; i--)
        {
            if (Heart[i].fillAmount != 0)
            {
                Heart[i].fillAmount -= 0.5f;
                break;
            }            
        }

    }//TakeDamage()

    void GetItem(Collision2D collision)
    {
        EffectMgr.Instance.PlayEffect(ItemAudio.name);

        Destroy(collision.gameObject);

        for (int i = 0; i <= Heart.Length - 1; i++)
        {
            if (Heart[i].fillAmount != 1 && ItemCheck != collision.gameObject && Heart[4].fillAmount < 1)
            {
                ItemCheck = collision.gameObject;

                if (Heart[4].fillAmount == 1)
                    return;

                if (Heart[i].fillAmount == 0.5f)
                {
                    if (Heart[4].fillAmount == 0.5f)
                        Heart[4].fillAmount += 0.5f;
                    else
                    {
                        Heart[i].fillAmount += 0.5f;
                        Heart[i + 1].fillAmount += 0.5f;
                    }
                }
                else if (Heart[i].fillAmount == 0f)
                {
                    Heart[i].fillAmount += 1f;
                }
                break;
            }//if (Heart[i].fillAmount != 1 && ItemCheck != collision.gameObject)
        }//for (int i = 0; i <= Heart.Length - 1; i++)
    }

    void Spring()
    {  
        Debug.DrawRay(rigid.position, Vector3.down, new Color(0, 1, 0));
        RaycastHit2D sp_ray = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Spring"));
                
        if (sp_ray.collider != null)
        {
            if (sp_ray.distance < 0.7f)
            {
                SpringController a_SpringController = FindObjectOfType<SpringController>();
                a_SpringController.SpringANi = true;
                rigid.AddForce(new Vector2(-0.1f, 5f), ForceMode2D.Impulse);//점프
                anim.SetBool("IsJumping", false);
                JumpCount = 0;
            }
        }
    }

    void Goto_Stage2()
    {
        Debug.DrawRay(rigid.position, Vector3.down, new Color(1, 1, 0));
        RaycastHit2D Ele_ray = Physics2D.Raycast(rigid.position, Vector3.down, 1, LayerMask.GetMask("Elevator"));

        if (Ele_ray.collider != null)
        {
            if (Ele_ray.distance < 0.7f)
            {
                GameManager.m_CamMove = true;
                anim.SetBool("IsJumping", false);
                JumpCount = 0;
            }
        }
    }

    void Goto_Stage3()
    {
        if(this.transform.position.x >=8f && GameManager.m_Stage == Stage.Stage2)
        {
            GameManager.m_CamMove = true;
            GameManager.m_GameState = GameState.CamMove;
        }

    }
    void Goto_Stage4()
    {
        if (GameManager.m_Stage == Stage.Stage3)
        {
            GameManager.m_CamMove = true;
            GameManager.m_GameState = GameState.CamMove;
        }

    }

    void Goto_Boss()
    {
        GameManager.m_CamMove = true;
        GameManager.m_GameState = GameState.CamMove;
        renderer.flipX = false;
    }

    float a = 1;
    void Die()
    {
        m_Playerstate = PlayerState.Die;
        if (a > 0)
            a -= Time.deltaTime*0.5f;           
        else
            GameManager.m_GameState = GameState.GameOver;

        renderer.color = new Color(1, 1, 1, a);    //점점 사라지기
    }

    public void Restrat()
    {
        for (int i = 0; i < 3; i++) //체력회복
            Heart[i].fillAmount = 1;
        this.transform.position = new Vector2(-6, 0);   //위치 조정       
        
    }
}
