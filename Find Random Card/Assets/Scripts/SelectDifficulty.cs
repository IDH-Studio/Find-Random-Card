using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SelectDifficulty : MonoBehaviour
{
    [SerializeField]
    private Scrollbar       _scrollBar;
    [SerializeField]
    private float           _swipeTime = 0.2f;
    [SerializeField]
    private float           _minSwipeDistance = 50;
    [SerializeField]
    private TextMeshProUGUI _showDifficultyInfo;

    [Space(20)]
    [Header("▼ Difficulty Info(Easy, Normal, Hard)")]
    [TextArea]
    [SerializeField] private string[] _infos;

    private float[]         _scrollPageValues;
    private float           _valueDistance = 0;
    private int             _currentPage = 0;
    private int             _maxPage = 0;
    private float           _startTouchX;
    private float           _endTouchX;
    private bool            _isSwipe;

    private void Awake()
    {
        _scrollPageValues = new float[transform.childCount];

        _valueDistance = 1f / (_scrollPageValues.Length - 1f);

        for (int index = 0; index < _scrollPageValues.Length; ++index)
        {
            _scrollPageValues[index] = _valueDistance * index;
        }

        _maxPage = transform.childCount;
    }

    private void Start()
    {
        SetScrollBarView(0);
    }

    public void SetScrollBarView(int index)
    {
        _currentPage = index;
        _scrollBar.value = _scrollPageValues[index];
        GameManager._instance.SelectDifficulty(_currentPage + 3);
    }

    private void Update()
    {
        if (_isSwipe) return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            _startTouchX = Input.mousePosition.x;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            _endTouchX = Input.mousePosition.x;
            UpdateSwipe();
        }
#endif

#if UNITY_ANDROID
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _startTouchX = touch.position.x;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                _endTouchX = touch.position.x;
                UpdateSwipe();
            }
        }
#endif

        ShowDifficultyInfo();
    }

    void UpdateSwipe()
    {
        if (Mathf.Abs(_startTouchX - _endTouchX) < _minSwipeDistance)
        {
            StartCoroutine(OnSwipeOneStep(_currentPage));
            return;
        }

        bool isLeft = _startTouchX < _endTouchX ? true : false;

        if (isLeft)
        {
            if (_currentPage == 0) return;

            _currentPage--;
        }
        else
        {
            if (_currentPage == _maxPage - 1) return;

            _currentPage++;
        }

        GameManager._instance.SelectDifficulty(_currentPage + 3);
        StartCoroutine(OnSwipeOneStep(_currentPage));
    }

    void ShowDifficultyInfo()
    {
        _showDifficultyInfo.text = _infos[_currentPage];
    }

    // 난이도 선택 화살표 버튼
    public void SelectPage(string direction)
    {
        if (direction == "next" && _currentPage < _maxPage - 1)
        {
            _currentPage++;
        }
        else if (direction == "prev" && _currentPage > 0)
        {
            _currentPage--;
        }

        GameManager._instance.SelectDifficulty(_currentPage + 3);
        ShowDifficultyInfo();
        StartCoroutine(OnSwipeOneStep(_currentPage));
    }

    // 난이도 클릭 시 버튼 이동
    public void ButtonClickMove(int difficulty)
    {
        // difficulty
        // 0: Easy, 1: Normal, 2: Hard
        ShowDifficultyInfo();
        _currentPage = difficulty;
        StartCoroutine(OnSwipeOneStep(_currentPage));
    }

    IEnumerator OnSwipeOneStep(int index)
    {
        float start = _scrollBar.value;
        float current = 0;
        float percent = 0;

        _isSwipe = true;

        while (percent < 1)
        {
            current += 0.01f;
            percent = current / _swipeTime;

            _scrollBar.value = Mathf.Lerp(start, _scrollPageValues[index], percent);

            yield return null;
        }

        _isSwipe = false;
    }
}
