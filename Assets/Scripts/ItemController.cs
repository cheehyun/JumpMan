using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Item_type
{
    IT_Apple,
    IT_Banana,
    IT_Cherry,
}

public class ItemValue
{
    public ulong UniqueID = 0;
    public Item_type m_ItemType;

    public ItemValue() { }
}

public class ItemController : MonoBehaviour
{
    [HideInInspector] public ItemValue m_ItemValue = new ItemValue();

    Rigidbody2D rigid;
    // Start is called before the first frame update
    void Start()
    {
        rigid = gameObject.GetComponent<Rigidbody2D>();

        //튕겨오르기
        Vector2 SpawnVel = Vector2.zero;

        SpawnVel = new Vector2(0f, 4f);

        rigid.AddForce(SpawnVel, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitItem(Item_type a_Item_Type)
    {
       
        //밀려남 처리
        //m_ItemValue.UniqueID = GlobalUserData.GetUnique();
        m_ItemValue.m_ItemType = a_Item_Type;
    }
}
