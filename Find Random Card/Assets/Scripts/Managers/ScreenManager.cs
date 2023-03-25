using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _screens;

    private IDictionary<string, GameObject> _screenDic;
    private Stack<string> _screenStack;
    private string _bgmScreenName; // 배경음악 재생 관련 변수

    private void Start()
    {
        _screenDic = new Dictionary<string, GameObject>();
        _screenStack = new Stack<string>();

        // 스크린 초기화
        foreach (GameObject screen in _screens)
        {
            _screenDic.Add(screen.name.Split(' ')[0], screen);
            screen.SetActive(false);
        }

        // 시작 메뉴만 활성화
        //_screenDic["Start"].SetActive(true);
        _screenStack.Push("Start");
        ShowScreen(true, _screenStack.Peek());
        _bgmScreenName = _screenStack.Peek();
    }

    void ShowScreen(bool isShow, string screenName)
    {
        _screenDic[screenName].SetActive(isShow);
    }

    public void ScreenClear()
    {
        // 가장 최근에 보여진 화면 없애기
        // 모든 화면 없애면서 활성화된 화면은 화면 끄고 없애기
        //ShowScreen(false, _screenStack.Peek());
        while(_screenStack.Count > 0)
        {
            ShowScreen(false, _screenStack.Pop());
        }

        // _screenStack 초기화
        _screenStack.Clear();
        _screenStack.Push("Start");
        ShowScreen(true, _screenStack.Peek());
        // 배경음악 재생
        BGMPlay(_screenStack.Peek());

    }


    // 화면 이동 관련 함수
    public void PrevScreen()
    {
        // 현재 화면을 스택에서 Pop하여 끈 뒤 가장 최근에 활성화된 화면을 다시 켠다.
        ShowScreen(false, _screenStack.Pop());
        ShowScreen(true, _screenStack.Peek());

        // 이동한 스크린에 맞는 배경음악 재생
        BGMPlay(_screenStack.Peek());
    }

    public void GoScreen(string screenName)
    {
        // 배경음악 재생
        BGMPlay(screenName);

        // 시작 메뉴 화면을 끄고 난이도 화면으로 넘어간다
        ShowScreen(false, _screenStack.Peek());
        _screenStack.Push(screenName);
        ShowScreen(true, _screenStack.Peek());

        if (screenName == "Setting") { GameManager._instance._settingManager.ShowSetting(); }
    }

    public void CoverScreen(string screenName)
    {
        // 배경음악 재생
        BGMPlay(screenName);

        _screenStack.Push(screenName);
        ShowScreen(true, _screenStack.Peek());
    }

    void BGMPlay(string screenName)
    {
        // 현재 재생중인 음악과 요청하는 음악이 같으면 그대로 재생
        if (_bgmScreenName == screenName) return;

        // 스크린에 맞는 음악 재생
        switch (screenName)
        {
            case "Start":
                GameManager._instance._soundManager.Play(false, "StartMusic");
                _bgmScreenName = "Start";
                break;
            case "Tutorial":
                GameManager._instance._soundManager.Play(false, "TutorialMusic");
                _bgmScreenName = "Tutorial";
                break;
            case "Preview":
                GameManager._instance._soundManager.Play(false, "PreviewMusic");
                _bgmScreenName = "Preview";
                break;
            case "Game":
                GameManager._instance._soundManager.Play(false, "GameMusic");
                _bgmScreenName = "Game";
                break;
        }
    }
}
