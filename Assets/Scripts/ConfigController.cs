using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfigController : MonoBehaviour
{
    GameManager m_RefGameManager;

    [Header("Button")]
    public Button Close_Btn;
    public Button LobbyBtn;

    [Header("Audio관련")]
    public Toggle SoundTog;
    public Toggle EffectTog;

    public Slider SoundSd;
    public Slider EffectSd;

    float BGMVolum = 0.5f;
    float EffectVolum = 0.5f;

    private void Awake()
    {
        if (GameManager.m_GameState == GameState.Play || GameManager.m_GameState == GameState.CamMove)
            LobbyBtn.gameObject.SetActive(true);
        else
            LobbyBtn.gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        if (SoundTog != null)
            SoundTog.onValueChanged.AddListener(SoundOnOff);

        if (EffectTog != null)
            EffectTog.onValueChanged.AddListener(EffectOnOff);

        if (SoundSd != null)
            SoundSd.onValueChanged.AddListener(SoundsdChange);

        if (EffectSd != null)
            EffectSd.onValueChanged.AddListener(EffectsdChange);

        //--- 각종 컨트롤들의 초기값 로딩 및 셋팅 부분
        int a_SoundOnOff = PlayerPrefs.GetInt("SoundOnOff", 1);
        if (SoundTog != null)
        {
            if (a_SoundOnOff == 0)
                SoundTog.isOn = false;
            else
                SoundTog.isOn = true;
        }

        int a_EffectOnOff = PlayerPrefs.GetInt("EffectOnOff", 1);
        if (EffectTog != null)
        {
            if (a_EffectOnOff == 0)
                EffectTog.isOn = false;
            else
                EffectTog.isOn = true;
        }

        float a_SoundV = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        if (SoundSd != null)
            SoundSd.value = a_SoundV;

        float a_EffectV = PlayerPrefs.GetFloat("EffectVolume", 1.0f);
        if (EffectSd != null)
            EffectSd.value = a_EffectV;
        //--- 각종 컨트롤들의 초기값 로딩 및 셋팅 부분


        if (Close_Btn != null)
            Close_Btn.onClick.AddListener(() =>
            {
                Time.timeScale = 1;
                Destroy(gameObject);
            });
        if (LobbyBtn != null)
            LobbyBtn.onClick.AddListener(() =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
            });        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SoundOnOff(bool value) //체크 상태가 변경되었을 때 호출되는 함수
    {
        if (value == true) //On
        {
            SoundMgr.Instance.SoundOnOff(true); //"사운드켜기"
            PlayerPrefs.SetInt("SoundOnOff", 1);
        }
        else   //Off
        {
            SoundMgr.Instance.SoundOnOff(false); //"사운드끄기"
            PlayerPrefs.SetInt("SoundOnOff", 0);
        }
    }

    public void EffectOnOff(bool value) //체크 상태가 변경되었을 때 호출되는 함수
    {
        if (value == true) //On
        {
            EffectMgr.Instance.EffectOnOff(true); //"효과음켜기"
            PlayerPrefs.SetInt("EffectOnOff", 1);
        }
        else   //Off
        {
            EffectMgr.Instance.EffectOnOff(false); //"효과음끄기"
            PlayerPrefs.SetInt("EffectOnOff", 0);
        }
    }

    public void SoundsdChange(float value)
    //value  0.0 ~ 1.0f //슬라이드 상태가 변경 되었을 때 호출되는 함수
    {
        SoundMgr.Instance.BGMVolume(value);
        PlayerPrefs.SetFloat("SoundVolume", value);
    }

    public void EffectsdChange(float value)
    //value  0.0 ~ 1.0f //슬라이드 상태가 변경 되었을 때 호출되는 함수
    {
        EffectMgr.Instance.EffectVolume(value);
        PlayerPrefs.SetFloat("EffectVolume", value);
    }
}
