using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    float m_LifeTime = 4.0f;
    Vector3 m_DirTgVec;
    Vector3 a_MoveNextStep;
    private float m_MoveSpeed = 5.0f;  //한프레임당 이동시키고 싶은 거리

    Vector3 m_OwnTrPos;
    Vector3 a_StartPos = new Vector3(0, 0, 0);

    BoxController m_BoxCtrl = null;


    // Start is called before the first frame update
    void Start()
    {
        m_BoxCtrl = FindObjectOfType<BoxController>();
    }

    // Update is called once per frame
    void Update()
    {
        m_LifeTime = m_LifeTime - Time.deltaTime;
        if (m_LifeTime <= 0.0f)
        {
            ResetState();
        }

        a_MoveNextStep = m_DirTgVec * (Time.deltaTime * m_MoveSpeed);
        a_MoveNextStep.y = 0.0f;

        transform.position = transform.position + a_MoveNextStep;       
    }

    public void ResetState()
    {
        m_LifeTime = 0;
        Destroy(this.gameObject);
    }

    public void BulletSpawn(Transform a_OwnTr, Vector3 a_DirVec)
    {
        m_OwnTrPos = a_OwnTr.position;

        m_DirTgVec = a_DirVec;
        m_DirTgVec.Normalize();

        a_StartPos = m_OwnTrPos + m_DirTgVec;

        transform.position = new Vector3(a_StartPos.x, a_StartPos.y, transform.position.z);

        if (m_DirTgVec == Vector3.left)     //발사 방향에따라 방향 회전 
            transform.localScale = new Vector3(transform.localScale.x, -transform.localScale.y, transform.localScale.z);

        m_LifeTime = 3f;

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name.Contains("Monster") == true)   //맞는객체
            {
            ResetState();
               MonsterController a_Enemy = other.gameObject.GetComponent<MonsterController>();

            if (a_Enemy != null)
            {
                a_Enemy.TakeDamage(10f);
            }
            
        }
        if (other.gameObject.name.Contains("BossMon") == true)   //맞는객체
        {
            ResetState();
            BossController a_Enemy = other.gameObject.GetComponent<BossController>();

            if (a_Enemy != null)
            {
                a_Enemy.TakeDamage(10f);
            }

        }


        if (other.gameObject.name.Contains("Box") == true)
        {
            ResetState();
            BoxController a_Box = other.gameObject.GetComponent<BoxController>();

            if (a_Box != null)
                a_Box.HitBox();
        }
        if (other.gameObject.name.Contains("DoorBtn") == true)
        {
            GameManager.m_DoorBtn_Click = true;
            Animator D_Anim = other.gameObject.GetComponent<Animator>();
            D_Anim.SetBool("IsClick", true);
        }

        Destroy(gameObject);
    }


}
