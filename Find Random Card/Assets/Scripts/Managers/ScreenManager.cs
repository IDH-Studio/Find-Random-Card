using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject[] screens;

    private IDictionary<string, GameObject> screenDic;
    private Stack<string> screenStack;

    private void Start()
    {
        screenDic = new Dictionary<string, GameObject>();
        screenStack = new Stack<string>();

        // 스크린 초기화
        foreach (GameObject screen in screens)
        {
            screenDic.Add(screen.name.Split(' ')[0], screen);
            screen.SetActive(false);
        }

        // 시작 메뉴만 활성화
        //screenDic["Start"].SetActive(true);
        screenStack.Push("Start");
        ShowScreen(true, screenStack.Peek());
    }

    void ShowScreen(bool isShow, string screenName)
    {
        screenDic[screenName].SetActive(isShow);
    }

    public void ScreenClear()
    {
        // 가장 최근에 보여진 화면 없애기
        ShowScreen(false, screenStack.Peek());

        // screenStack 초기화
        screenStack.Clear();
        screenStack.Push("Start");
        ShowScreen(true, screenStack.Peek());
    }

    public void PrevScreen()
    {
        // 현재 화면을 스택에서 Pop하여 끈 뒤 가장 최근에 활성화된 화면을 다시 켠다.
        ShowScreen(false, screenStack.Pop());
        ShowScreen(true, screenStack.Peek());
    }

    public void GoScreen(string screenName)
    {
        // 시작 메뉴 화면을 끄고 난이도 화면으로 넘어간다
        ShowScreen(false, screenStack.Peek());
        screenStack.Push(screenName);
        ShowScreen(true, screenStack.Peek());
    }
}
