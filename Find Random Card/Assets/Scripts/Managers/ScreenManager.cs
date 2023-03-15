using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject[] _screens;

    private IDictionary<string, GameObject> _screenDic;
    private Stack<string> _screenStack;

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
    }

    public void PrevScreen()
    {
        // 현재 화면을 스택에서 Pop하여 끈 뒤 가장 최근에 활성화된 화면을 다시 켠다.
        ShowScreen(false, _screenStack.Pop());
        ShowScreen(true, _screenStack.Peek());
    }

    public void GoScreen(string screenName)
    {
        // 시작 메뉴 화면을 끄고 난이도 화면으로 넘어간다
        ShowScreen(false, _screenStack.Peek());
        _screenStack.Push(screenName);
        ShowScreen(true, _screenStack.Peek());
    }

    public void CoverScreen(string screenName)
    {
        _screenStack.Push(screenName);
        ShowScreen(true, _screenStack.Peek());
    }
}
