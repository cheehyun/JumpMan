using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxController : MonoBehaviour
{
    Animator anim;

    public int m_HitCount = 0;

    public AudioClip BreakBoxClip;


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        m_HitCount = 0;
    }

    // Update is called once per frame
    void Update()
    {       
        if (m_HitCount >= 2)
        {
            DestroyBox();
        }
    }

    public void HitBox()
    {
        anim.SetTrigger("Hit");     
        m_HitCount++;

        EffectMgr.Instance.PlayEffect(BreakBoxClip.name);

        if (m_HitCount >= 2)
        {
            DestroyBox();
        }
    }

    void DestroyBox()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("BoxHit") == true && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            EffectMgr.Instance.PlayEffect(BreakBoxClip.name);

            Destroy(gameObject);

            ItemDrop();
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


}
