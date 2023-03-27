using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPages : MonoBehaviour
{
    [SerializeField] private GameObject[]   _pages;

    [Space(10)]
    [Header("▼ Buttons")]
    [SerializeField] private Image        _nextButton;
    [SerializeField] private Image        _prevButton;

    private int                           _currentPageIndex = 0;
    private float                         _deactiveColor = 80 / 255f;
    private float                         _activeColor = 1f;
    private float                         _color;

    private void Awake()
    {
        foreach (GameObject page in _pages)
        {
            page.SetActive(false);
        }

        _pages[0].SetActive(true);
    }

    private void OnEnable()
    {
        _pages[_currentPageIndex].SetActive(false);
        _currentPageIndex = 0;
        _pages[_currentPageIndex].SetActive(true);
        _nextButton.color = new Color(_activeColor, _activeColor, _activeColor);
        _prevButton.color = new Color(_deactiveColor, _deactiveColor, _deactiveColor);
    }

    public void NextPage()
    {
        // 현재 페이지(_currentPageIndex)가 마지막 페이지(_pages.Length - 1)일 경우 작동하지 않음
        if (_currentPageIndex == _pages.Length - 1) return;

        _pages[_currentPageIndex++].SetActive(false);
        _pages[_currentPageIndex].SetActive(true);

        _color = (_currentPageIndex == _pages.Length - 1) ? _deactiveColor : _activeColor;
        _nextButton.color = new Color(_color, _color, _color);
        _prevButton.color = new Color(_activeColor, _activeColor, _activeColor);
    }

    public void PrevPage()
    {
        // 현재 페이지(_currentPageIndex)가 첫 번째 페이지(0)일 경우 작동하지 않음
        if (_currentPageIndex == 0) return;

        _pages[_currentPageIndex--].SetActive(false);
        _pages[_currentPageIndex].SetActive(true);

        _color = (_currentPageIndex == 0) ? _deactiveColor : _activeColor;
        _prevButton.color = new Color(_color, _color, _color);
        _nextButton.color = new Color(_activeColor, _activeColor, _activeColor);
    }

    public void StopTutorial()
    {
        GameManager._instance._screenManager.PrevScreen();
        GameManager._instance._soundManager.PlayEffectSound("ButtonPopSoundDown");
    }
}
