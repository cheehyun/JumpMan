using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonType
{
    M_Level1,
    M_Level2,
};

public class MonsterController : MonoBehaviour
{
    GameManager m_GameManager = null;
    MonType m_MonType = MonType.M_Level1;

    public float MovePower = 1f;

    float FirstPosition; //처음 위치(x) 저장

    Animator animator;
    Rigidbody2D rigid;
    int movementFlag = 0;   //0:Idle, 1:Left, 2:Right
    int a_Count = 0;

    float m_CurHP = 0;
    float m_MaxHP = 0;

    bool isDie = false;

    [Header("Audio")]
    public AudioClip KillMonClip;


    // Start is called before the first frame update
    void Start()
    {
        m_GameManager = FindObjectOfType<GameManager>();
        FirstPosition = transform.position.x;
        animator = gameObject.GetComponentInChildren<Animator>();
        rigid = gameObject.GetComponent<Rigidbody2D>();
        StartCoroutine("ChangeMovement");

        m_MaxHP = 30;
        m_CurHP = 30;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Move();

        if (this.rigid.velocity != Vector2.zero)
            animator.SetBool("IsMoving", true);
    }

    private void Update()
    {
        if (FirstPosition + 1.5f <= transform.position.x)
        {
            transform.position = new Vector2(FirstPosition + 1.5f, transform.position.y);
            movementFlag = Random.Range(0, 3);
        }
        else if (FirstPosition - 1.5f >= transform.position.x)
        {
            transform.position = new Vector2(FirstPosition - 1.5f, transform.position.y);
            movementFlag = Random.Range(0, 3);
        }

        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Mon_Hit") == true && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
        {
            animator.SetBool("Hit", false);
        }
    }

    void Move()
    {
        Vector3 moveVelocity = Vector3.zero;
        if (movementFlag == 1)  //왼쪽
        {
            moveVelocity = Vector3.left;
            transform.localScale = new Vector3(-1.2f, 1.2f, 1.2f);
        }
        else if (movementFlag == 2) //오른쪾
        {
            moveVelocity = Vector3.right;
            transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
        }
        else if(movementFlag == 3 || movementFlag == 4)  //왼쪽점프
        {
            if(movementFlag == 3)
                moveVelocity = Vector3.left;
            else if(movementFlag == 4)
                moveVelocity = Vector3.right;

            transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            if (a_Count != movementFlag)
            {
                a_Count = movementFlag;
                this.rigid.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
            }

            if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.5f)
                animator.SetBool("IsJumping", false);
        }
        transform.position += moveVelocity * MovePower * Time.deltaTime;
        
    }

    IEnumerator ChangeMovement()
    {
        if(m_MonType == MonType.M_Level1)
            movementFlag = Random.Range(0, 3);  //움직임 랜덤    //Stage1에서는 걷기, 쉬기
        else if(m_MonType == MonType.M_Level2)
            movementFlag = Random.Range(0, 5);  //Stage2에서는 걷기, 쉬기, 점프

        a_Count = 0;

        //애니메이션 움직임
        if (movementFlag == 0)
        {
            animator.SetBool("IsMoving", false);
           // animator.SetBool("IsJumping", false);
        }
        else if (movementFlag == 1 || movementFlag == 2)
        {
            animator.SetBool("IsMoving", true);
            //animator.SetBool("IsJumping", false);
        }
        else if (movementFlag == 3 || movementFlag == 4)
        {
            animator.SetBool("IsJumping", true);
        }


        yield return new WaitForSeconds(3f);

        StartCoroutine("ChangeMovement");//다시호출
    }

    void Die()
    {
        StopCoroutine("ChangeMovement");
        isDie = true;
            
        ItemDrop();

        m_GameManager.a_MonCount++;        
        Destroy(gameObject);
    }

    public void TakeDamage(float a_Val)
    {
        animator.SetTrigger("Hit");

        EffectMgr.Instance.PlayEffect(KillMonClip.name);

        if (m_CurHP > 0)
        m_CurHP -= a_Val;

        if(m_CurHP <= 0)
        {
            m_CurHP = 0;
            Die();
        }
    }

    void ItemDrop()
    {
        int a_Rnd = Random.Range(0, 3);
        if (0 <= a_Rnd && a_Rnd < 3)
        {
            int a_TexInx = a_Rnd;
            string a_ObjName = "Item";
            if (a_Rnd == 0)
            {
                a_ObjName = "Apple";
            }
            else if (a_Rnd == 1)
            {
                a_ObjName = "Banana";
            }
            else if (a_Rnd == 2)
            {
                a_ObjName = "Cherry";
            }

            GameObject m_Item = null;
            m_Item = (GameObject)Instantiate(Resources.Load("Prefabs/" + a_ObjName));
            m_Item.transform.position = this.transform.position;


            m_Item.name = a_ObjName;

            ItemController a_RefItemInfo = m_Item.GetComponent<ItemController>();
            if (a_RefItemInfo != null)
            {
                a_RefItemInfo.InitItem((Item_type)a_TexInx);
            }

            Destroy(m_Item.gameObject, 10.0f);

        }
    }

    public void MonSpawn(MonType a_MonType)
    {
        m_MonType = a_MonType;
    }

}
