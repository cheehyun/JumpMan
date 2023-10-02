using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringController : MonoBehaviour
{
    Animator anim;

    public bool SpringANi = false;
    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (SpringANi == true)
        {
            this.anim.SetBool("SP_Jump", true);

            SpringANi = false;
        }

        if (this.gameObject.activeSelf == true)
            SpringCheck(); //스프링 애니메이션 끄기
    }

    void SpringCheck()
    {
        if (this.anim.GetCurrentAnimatorStateInfo(0).IsName("Sp_Jump") == true && this.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.99f)
            this.anim.SetBool("SP_Jump", false);
    }
}
