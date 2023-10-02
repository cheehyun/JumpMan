using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CameraController : MonoBehaviour
{
    GameManager m_GameMgr;

    Vector3 CacVec = Vector3.zero;
    Vector3 TrVec = Vector3.zero;
    GameObject a_Player = null;


    // Start is called before the first frame update
    void Start()
    {
        m_GameMgr = FindObjectOfType<GameManager>();
        CacVec = this.transform.position;
        a_Player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {      
        if(GameManager.m_CamMove == true)
            CameraMove(GameManager.m_Stage);
    }
    private void FixedUpdate()
    {        
        if (GameManager.m_Stage == Stage.Boss)
        {
            if (a_Player.transform.position.x >= 37 && a_Player.transform.position.x <= 55 && a_Player.transform.position.y >= -4 && a_Player.transform.position.y <= 10)
            {                
                if (a_Player != null)
                    this.transform.position = new Vector3(a_Player.transform.position.x, this.transform.position.y, this.transform.position.z);
                
            }
        }
    }

    void CameraMove(Stage a_Stage)
    {
        if (a_Stage == Stage.Stage1)
        {
            TrVec = new Vector3(0, 10, -10);
            Vector3 CacDir = TrVec - CacVec;
            CacDir = CacDir.normalized;
            this.transform.Translate(CacDir * Time.deltaTime * 5);

            if (this.transform.position.y >= TrVec.y)
            {
                GameManager.m_CamMove = false;
                GameManager.m_GameState = GameState.Play;
                GameManager.m_Stage = Stage.Stage2;
                CacVec = TrVec;

                Destroy(GameObject.Find("Stage1"));
            }
        }
        if (a_Stage == Stage.Stage2)
        {
            TrVec = new Vector3(18f, 10, -10);
            Vector3 CacDir = TrVec - CacVec;
            CacDir = CacDir.normalized;

            this.transform.Translate(CacDir * Time.deltaTime * 5);

            if (this.transform.position.x >= TrVec.x)
            {
                GameManager.m_CamMove = false;
                GameManager.m_GameState = GameState.Play;
                GameManager.m_Stage = Stage.Stage3;
                GameManager.m_DoorBtn_Click = false;
                CacVec = TrVec;

                Destroy(GameObject.Find("Stage2"));
            }
        }
        if (a_Stage == Stage.Stage3)
        {
            TrVec = new Vector3(18f, 0, -10);
            Vector3 CacDir = TrVec - CacVec;
            CacDir = CacDir.normalized;

            this.transform.Translate(CacDir * Time.deltaTime * 5);

            if (this.transform.position.y <= TrVec.y)
            {
                GameManager.m_CamMove = false;
                GameManager.m_GameState = GameState.Play;
                GameManager.m_Stage = Stage.Stage4;
                CacVec = TrVec;

                Destroy(GameObject.Find("Stage3"));
            }
        }
        if (a_Stage == Stage.Stage4)
        {
            TrVec = new Vector3(37f, 0, -10);
            this.transform.position = TrVec;

            if (this.transform.position == TrVec)
            {
                GameManager.m_Stage = Stage.Boss;
                BossController.m_BossState = BossState.Start;
                CacVec = TrVec;

                Destroy(GameObject.Find("Stage4"));
            }
        }
        
    }
}
