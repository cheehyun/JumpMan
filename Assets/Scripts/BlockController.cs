using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum BlockType
{
    Fade,
    Repeat,
    Trap,
}

public class BlockController : MonoBehaviour
{
    BlockType m_BlockType = BlockType.Fade;

    //Fade In Out 변수
    float m_AddTime = 0;
    float m_CacTime = 0;
    float m_StVal = 1f;
    float m_EndVal = 0;
    //bool m_Fade = false;

    private Color m_Color;
    //Fade In Out 변수

    //좌우 이동 관련 변수
    Vector3 m_CurPos; //현재위치
    float m_Delta = 2.0f; // 좌(우)로 이동가능한 (x)최대값
    float m_Speed = 3.0f; // 이동속도
    //좌우 이동 관련 변수

    // Start is called before the first frame update
    void Start()
    {
        m_StVal = 1.0f;
        m_EndVal = 0.0f;
        if(this.gameObject.name.Contains("Saw"))
        {
            this.m_BlockType = BlockType.Trap;
            m_CurPos = this.transform.position;
        }
        else if (this.gameObject.name == "Block")
        {
            this.m_BlockType = BlockType.Repeat;
            m_CurPos = this.transform.position;
        }
        else
        {
            this.m_BlockType = BlockType.Fade;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (m_BlockType == BlockType.Fade)
        {
            StartCoroutine(FadeInOut());

            if (m_Color.a < 0.3f)
                this.GetComponent<Collider2D>().enabled = false;
            else
                this.GetComponent<Collider2D>().enabled = true;
        }
        else if (m_BlockType == BlockType.Repeat)
        {
            Repeat();
        }
        else if(m_BlockType == BlockType.Trap)
        {
            Trap();
        }

        
    }
    IEnumerator FadeInOut()
    {
        if (m_CacTime < 1.0f)   //페이드 끝
        {
            m_AddTime += Time.deltaTime;
            m_CacTime = m_AddTime / 0.8f;
            m_Color = this.GetComponent<SpriteRenderer>().color;
            m_Color.a = Mathf.Lerp(m_StVal, m_EndVal, m_CacTime);  //Lerp-> 도달함수[비율로 환산] //사라지기
            this.GetComponent<SpriteRenderer>().color = m_Color;
            if (m_CacTime >= 1f)
            {
                if (m_StVal == 1f && m_EndVal == 0)   //사라졌을때
                {
                    yield return new WaitForSeconds(1.5f);
                    FadeOut();                    
                }
                else if (m_StVal == 0 && m_EndVal == 1f) //나타났을때
                {
                    yield return new WaitForSeconds(1.5f);
                    FadeIn();
                }
            }
        }
    }

    void FadeIn()
    {
        m_Color.a = 1f;
        m_CacTime = 0f;
        m_AddTime = 0f;
        m_StVal = 1;
        m_EndVal = 0f;
    }

    void FadeOut()
    {
        m_Color.a = 0f;
        m_CacTime = 0f;
        m_AddTime = 0f;
        m_StVal = 0;
        m_EndVal = 1f;

    }

    void Repeat()
    {
        Vector3 v = m_CurPos;

        v.x += m_Delta * Mathf.Sin(Time.time * 2);

        transform.position = v;
    }

    void Trap()
    {
        Vector3 v = m_CurPos;

        v.y += 0.5f * Mathf.Sin(Time.time);

        transform.position = v;
    }
}
