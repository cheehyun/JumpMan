using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public enum GameState
{
    Ready,
    Play,
    CamMove,
    GameOver,
    End,
}

public enum Stage
{
    Stage1,
    Stage2,
    Stage3,
    Stage4,
    Boss
}

public class GameManager : MonoBehaviour
{
    [HideInInspector] public PlayerController m_Player = null;

    public static GameState m_GameState = GameState.Ready;
    public static Stage m_Stage = Stage.Stage1;
    public static bool m_CamMove = false;

    float m_AttckSpeed = 0.7f;      //주인공 공속
    float m_CacAtTick = 0f;         //기관총 발사 틱 만들기

    [HideInInspector] public int a_MonCount;
    [HideInInspector] public int m_MonCount;

    //환경설정 변수
    [Header("------------Config--------------")]
    public Button m_Opt_Btn;
    public Button m_Opt_Btn2;
    public GameObject Canvas_Dialog = null;
    private GameObject m_ConfigBoxObj = null;
    //환경설정 변수

    [Header("------------Lobby--------------")]
    public Button m_Start_Btn;
    public GameObject Lobby_Canvas = null;

    [Header("------------Stage1--------------")]
    //Spring
    public GameObject Spring = null;
    public GameObject Box = null;
    public GameObject m_Sparkle = null;
    bool SparkleActive = false;

    GameObject a_Sparkle;
    Animator sk_Ani = null;
    //Spring

    [Header("------------Stage2--------------")]
    public static bool m_DoorBtn_Click = false;
    public GameObject Door = null;
    public GameObject Monster2 = null;

    [Header("------------Stage3--------------")]
    public Transform RepeatBlock = null;

    [Header("------------Stage4--------------")]
    public Light2D m_GlobalLight;
    public Light2D m_PlayerLight;
    public Light2D m_PotalLight;

    [Header("------------Boss--------------")]
    public static bool m_BossStage = false;
    public GameObject m_BossHP;
    public Image BossHP_Bar;
    BossController m_Boss;

    [Header("------------End--------------")]
    public Image m_GameEnd_Panel;
    public Text m_Title_Txt;
    public Text m_Body_Txt;
    public Button m_Lobby_Btn;

    [Header("------------GameOver--------------")]
    public Image m_GameOver_Panel;
    public Button m_Yes_Btn;

    [Header("------------Audio------------")]
    public AudioClip TitleAuido;
    public AudioClip InGameAudio;
    public AudioClip GameOverAudio;
    public AudioClip GameClearAudio;
    public AudioClip BossAudio;
    public AudioClip TypingAudio;

    bool AudioPlay = false;

    [Header("------------EffectAudio------------")]
    public AudioClip UIBtnClip;
    public AudioClip SparkleClip;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        m_Player = FindObjectOfType<PlayerController>();
        m_Boss = FindObjectOfType<BossController>();

        SoundMgr.Instance.PlayBGM(TitleAuido.name);

        //-------------환경설정 부분
        if (m_Opt_Btn != null)
            m_Opt_Btn.onClick.AddListener(()=>
            {
                Option();
                Time.timeScale = 0;
            });

        if (m_Opt_Btn2 != null)
            m_Opt_Btn2.onClick.AddListener(Option);
        //-------------환경설정 부분

        if (m_Start_Btn != null)
            m_Start_Btn.onClick.AddListener(Play);

        if (m_Lobby_Btn != null)
            m_Lobby_Btn.onClick.AddListener(ReStart);

        if(m_Yes_Btn != null)
            m_Yes_Btn.onClick.AddListener(ReStart);

    }

    // Update is called once per frame
    public void Update()
    {
        BossHP_Bar.fillAmount = m_Boss.m_CurHp / m_Boss.m_MaxHp;//Boss 체력

        m_CacAtTick -= Time.deltaTime;
        if (m_CacAtTick <= 0f)
            m_CacAtTick = 0f;

        //총알 발사 코드
        if (Time.timeScale > 0f && m_CacAtTick <= 0)
            if (Input.GetKeyDown(KeyCode.Space))  //스페이스 버튼 클릭
            {
                m_Player.Atteck();               
                
                m_CacAtTick = m_AttckSpeed;                
            }
        //총알 발사 코드

        if (SparkleActive == true)
        SparkleCheck();

        //스프링 생성
        if (a_MonCount == 2 && m_Stage == Stage.Stage1)
        {
            m_MonCount = a_MonCount;
            SpringSpawn();
            a_MonCount = 0;
        }
        //스프링 생성

        NextStage();

        //Door Open
        if (Door != null && m_DoorBtn_Click == true)
        {
            if (Door.transform.position.y < 9.3f)
            {
                Door.GetComponent<Collider2D>().enabled = false;
                Door.transform.Translate(Time.deltaTime * 0.5f, 0, 0f);
            }
            else
                m_DoorBtn_Click = false;
        }
        if(Door != null && m_Player.transform.position.x > 9.15f)
        {
            if (Door.transform.position.y >= 7.5f)
            {
                Door.GetComponent<Collider2D>().enabled = true;
                Door.transform.Translate(-Time.deltaTime * 2, 0, 0f);
            }
        }
        //Door Open

        //움직이는발판 따라가기
        if (PlayerController.m_RepeatBlock == true)
        {
            //m_Player.gameObject.transform.SetParent(RepeatBlock.transform, false);
            m_Player.transform.parent = RepeatBlock.transform;
        }
        else
        {
           m_Player.transform.parent = null;
        }
        //움직이는발판 따라가기

        //GameEnd
        if (m_GameState == GameState.End)
        //if(BossController.m_BossState == BossState.Death)
            GameEnd();
        //GameEnd
        //GameOver
        if (m_GameState == GameState.GameOver)
            GameOver();
        //GameOver

    }

    void Play()
    {
        Lobby_Canvas.SetActive(false);
        m_GameState = GameState.Play;

        SoundMgr.Instance.PlayBGM(InGameAudio.name);
    }

    void Option()
    {
        if (m_ConfigBoxObj == null)
            m_ConfigBoxObj = Resources.Load("Prefabs/Config_Box") as GameObject;

        GameObject a_CfgBoxObj = (GameObject)Instantiate(m_ConfigBoxObj);
        a_CfgBoxObj.transform.SetParent(Canvas_Dialog.transform, false);
        //false로 해야 로컬 프리즘에 설정된 좌표를 유지한체 차일드로 붙게된다.
        //flase하지 않으면 부모 좌표로 자동설정
    }

    void SparkleCheck()
    {
        if (sk_Ani.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.9f)
        {
            Destroy(a_Sparkle.gameObject);
            GameObject a_Spring;
            a_Spring = (GameObject)Instantiate(Spring, new Vector3(-1.44f,0.47f,0), Quaternion.identity);
            SparkleActive = false;
        }
    }

    void SpringSpawn()
    {        
        if (SparkleActive == false)
        {
            SparkleActive = true;
            a_Sparkle = (GameObject)Instantiate(m_Sparkle, new Vector3(-1.44f, 0.47f, 0), Quaternion.identity);

            EffectMgr.Instance.PlayEffect(SparkleClip.name);
        }

        sk_Ani = a_Sparkle.gameObject.GetComponent<Animator>();

                
    }

    void NextStage()
    {
        if (m_CamMove == true)
        {
            //GameObject a_Player = GameObject.Find("Player");
            //Animator a_Anim = a_Player.GetComponent<Animator>();

            GameObject ele = GameObject.Find("Elevator");
            m_GameState = GameState.CamMove;

            //Stage2 Tile맵 Collider활성화
            //엘레베이터 이동
            if (m_Stage == Stage.Stage1)
            {
                if (ele != null)
                    if (ele.transform.position.y <= 6.45f)
                    {
                        ele.transform.Translate(Vector3.up * 0.05f);
                        //a_Anim.SetBool("IsMoving", false);
                    }
                    else
                    {
                        //stage2
                        GameObject Mon2 = (GameObject)Instantiate(Resources.Load("Prefabs/Monster2"), new Vector3(2, 6.6f, 0), Quaternion.identity);
                        Mon2.transform.parent = GameObject.Find("Stage2").transform;
                        MonsterController a_Mon2 = Mon2.GetComponent<MonsterController>();
                        a_Mon2.MonSpawn(MonType.M_Level2);
                        //몬스터 생성

                        //스프링 생성
                        GameObject a_Spring = (GameObject)Instantiate(Spring);
                        a_Spring.transform.parent = GameObject.Find("Stage2").transform;
                        //스프링 생성

                        Destroy(ele);
                        GameObject TileMap2 = GameObject.Find("Stage2_Tile");
                        TileMap2.GetComponent<TilemapCollider2D>().enabled = true;

                    }
            }// if (m_Stage == Stage.Stage1)
            if (m_Stage == Stage.Stage2)
            {

                if (m_Player.transform.position.x < 9.5f)
                {
                    m_Player.transform.Translate(Time.deltaTime * 0.5f, 0, 0);    //Stage3로 강제이동

                    //a_Anim.SetBool("IsMoving", true);
                }
                else
                {

                }

            }//if (m_Stage == Stage.Stage2)
            if (m_Stage == Stage.Stage3)
            {
                //Light조절
                if(m_GlobalLight.intensity >0)
                m_GlobalLight.intensity -= Time.deltaTime;

                if (m_PlayerLight.intensity < 1)
                    m_PlayerLight.intensity += Time.deltaTime;
                              
                //Light조절

            }           

        }// if (m_CamMove == true)
        //else if(m_CamMove ==false && BossController.m_BossState != BossState.Death) m_GameState = GameState.Play;

        if (m_BossStage == true)
        {
            if (!AudioPlay)
            {
                SoundMgr.Instance.PlayBGM(BossAudio.name, true);
                AudioPlay = true;
            }

            m_Player.transform.position = new Vector3(30, -3.4f, 0);

            //Light조절
            if (m_GlobalLight.intensity < 1)
                m_GlobalLight.intensity += Time.deltaTime;
            //Light조절

            m_PlayerLight.gameObject.SetActive(false);

            m_BossHP.SetActive(true);

            if (m_GlobalLight.intensity >= 1)
            {                
                m_GlobalLight.intensity = 1;
                m_PlayerLight.intensity = 0;
                m_BossStage = false;
                AudioPlay = false;
            }
        }
    }

    bool Typing = false;
    float a = 0;
    float delay = 0;
    void GameEnd()
    {
        if (m_GameState != GameState.End)
            return;

        if (!AudioPlay)
        {
            SoundMgr.Instance.PlayBGM(GameClearAudio.name, false);
            AudioPlay = true;
        }

        if (delay < 2)
            delay += Time.deltaTime;
        else
        {
            m_GameEnd_Panel.gameObject.SetActive(true);
        }

        if(m_GameEnd_Panel.gameObject.activeSelf == true)
        if (m_GameEnd_Panel.gameObject.GetComponent<Image>().color.a < 1)
        {
            a += Time.deltaTime * 0.5f; 
            m_GameEnd_Panel.gameObject.GetComponent<Image>().color = new Color(0, 0, 0, a);
        }

        if(a >= 1)
        {
            m_Title_Txt.gameObject.SetActive(true);
            m_Body_Txt.gameObject.SetActive(true);            
        }

        if (m_Body_Txt.gameObject.activeSelf == true)
        {
            if (!Typing)
            {
                EffectMgr.Instance.PlayEffect(TypingAudio.name);
                Typing = true;
            }
        }

        if(m_Body_Txt.gameObject.activeSelf == true)
        if(a < 4f)
            a += Time.deltaTime;
        if(a > 4f)
        {
            m_Lobby_Btn.gameObject.SetActive(true);
        }
    }
    
    
    public void ReStart()
    {
        //설정값 초기화
        m_GameState = GameState.Ready;
        m_Stage = Stage.Stage1;
        m_MonCount = 0;
        AudioPlay = false;
        //설정값 초기화     

        m_Player.Restrat();//플레이어 리셋

        Lobby_Canvas.SetActive(true);
        m_GameEnd_Panel.gameObject.SetActive(false);
        m_GameOver_Panel.gameObject.SetActive(false);
        m_BossHP.SetActive(false);  //Bosshp 감추기

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
        
    void GameOver()
    {
        m_GameOver_Panel.gameObject.SetActive(true);

        if (!AudioPlay)
        {
            SoundMgr.Instance.PlayBGM(GameOverAudio.name, false);
            AudioPlay = true;
        }

        if (a < 1)
        {
            a += Time.deltaTime;
            m_GameOver_Panel.color = new Color(0, 0, 0, a);

        }
        else
        {
            m_GameOver_Panel.color = new Color(0, 0, 0, 1);            
            //Time.timeScale = 0;
            //a = 0;
        }
    }

    public void BtnClick()
    {
        EffectMgr.Instance.PlayEffect(UIBtnClip.name);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
