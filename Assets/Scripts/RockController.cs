using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockController : MonoBehaviour
{
    float m_LifeTime = 4.0f;
    Vector3 m_DirTgVec;
    private float m_MoveSpeed = 20.0f;  //한프레임당 이동시키고 싶은 거리

    Vector3 m_OwnTrPos;
    Vector3 a_StartPos = new Vector3(0, 0, 0);

    bool Shoot = false;

    Rigidbody2D rigid;

    // Start is called before the first frame update
    void Start()
    {
        rigid = this.gameObject.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        m_LifeTime = m_LifeTime - Time.deltaTime;
        if (m_LifeTime <= 0.0f)
        {
            ResetState();
        }              

        if (!Shoot)
        {
            rigid.AddForce(m_DirTgVec * m_MoveSpeed, ForceMode2D.Impulse);
            Shoot = true;
        }
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
        //m_DirTgVec.Normalize();

        a_StartPos = m_OwnTrPos + m_DirTgVec;

        transform.position = new Vector3(a_StartPos.x, a_StartPos.y, transform.position.z);               

        m_LifeTime = 3f;

    }

    void OnTriggerEnter2D(Collider2D other)
    {        
        if (other.gameObject.name.Contains("Player") == true)   //맞는객체
        {
            ResetState();
            PlayerController a_Player = other.gameObject.GetComponent<PlayerController>();

            if (a_Player != null)
            {
                a_Player.TakeDamage();
            }

        }        

        if(other.gameObject.name != "BossMon")
            Destroy(gameObject);
    }
}
