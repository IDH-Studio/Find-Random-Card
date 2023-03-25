using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPages : MonoBehaviour
{
    [SerializeField] private GameObject[] _pages;

    private int _currentPageIndex = 0;

    private void Awake()
    {
        foreach (GameObject page in _pages)
        {
            page.SetActive(false);
        }

        _pages[0].SetActive(true);
    }
        
    public void NextPage()
    {
        // 현재 페이지(_currentPageIndex)가 마지막 페이지(_pages.Length - 1)일 경우 작동하지 않음
        if (_currentPageIndex == _pages.Length - 1) return;

        _pages[_currentPageIndex++].SetActive(false);
        _pages[_currentPageIndex].SetActive(true);
    }

    public void PrevPage()
    {
        // 현재 페이지(_currentPageIndex)가 첫 번째 페이지(0)일 경우 작동하지 않음
        if (_currentPageIndex == 0) return;

        _pages[_currentPageIndex--].SetActive(false);
        _pages[_currentPageIndex].SetActive(true);
    }

    public void StopTutorial()
    {
        _currentPageIndex = 0;

        GameManager._instance._screenManager.PrevScreen();
        GameManager._instance._soundManager.PlayEffectSound("ButtonPopSoundDown");
    }
}
