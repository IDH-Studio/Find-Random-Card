using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TMPro.Examples;
using Unity.Mathematics;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/*
 * 게임의 난이도는 Easy, Normal, Hard로 나뉜다
 * Easy, Normal, Hard 순으로 3x3, 4x4, 5x5로 카드의 수가 나뉘게 되고
 * Easy는 카드 보기가 30sec, Normal은 20sec, Hard는 10sec
 * 만약 바로 진행하고 싶다면 진행 버튼을 눌러 바로 진행하도록 한다.
 * 게임이 진행되면 카드는 전부 뒷면으로 바뀌게 된다.
 * 찾아야 하는 카드를 보여주고 해당 카드를 바로 찾으면 Fever Gauge가 한 칸 올라가고
 * Fever Gauge 5칸이 모두 차게 되면 피버 타임이 진행되고 피버 타임일 때엔 모든 카드를 다 앞면으로 바꾼다.
 * 만약 다른 카드를 선택 시 Fever Gauge가 한 칸 줄어들게 되고 (미정) -> 만약 난이도가 Hard면 FG를 다 없앤다.
 * 게임 시간은 30초, 1분, 1분 30초 중 하나로 진행하고 or 선택 하도록 하고 -> 1분으로 통일
 * 게임 시간이 모두 지나면 점수를 저장한다. (DB, Firebase, etc.. 이용)
 * 
 * 더 추가하고 싶다면 점수로 코인을 모아 뒷면 스킨을 살 수 있도록 한다.
*/

// Grid Size에 따라 DIFFICULTY가 달라짐
public enum DIFFICULTY
{
    EASY = 3,
    NORMAL = 4,
    HARD = 5,
}

/*
 * TODO
 * 게임 기능 제작 (프로토타입)
    ㄴ 총 카드 종류 : 30종류
 * 연결 할거 연결하고
 * 꾸밀꺼 꾸미고
 * 추가할거 추가하고
*/

public class GameManager : MonoBehaviour
{
    public static GameManager                   _instance;

    // 게임 관련 변수
    private DIFFICULTY                          _difficulty;

    // 게임 
    [Header("▼ Variables")]
    [SerializeField] private float              _maxGameTime = 60f;
    private float                               _gameTime;
    private bool                                _isGame = false;

    // 미리보기
    private float                               _maxPreviewTime;
    private float                               _previewTime;
    private bool                                _isPreview = false;

    // 콤보 (피버)
    [SerializeField] private int                _maxComboStack = 5;
    private int                                 _comboStack = 0;
    private float                               _maxFeverTime;
    private float                               _feverTime = 0;
    private bool                                _isFever = false;

    // Getter, Setter
    public DIFFICULTY                           Difficulty { get { return _difficulty; } }
    public bool                                 IsFever { get { return _isFever; } }


    // 카드 관련 변수
    [SerializeField] private int                _maxCardTypeCount;
    private List<CardInfo>                      _cards;
    private List<CardInfo>                      _newCards;

    private CardInfo                            _findCard;
    private List<Card>                          _curCards;

    private GridLayoutGroup                     _cardLayoutGroup;
    private int                                 _gridSize;
    private float                               _flipCardSize;

    // Show In Inspector
    [Header("▼ Objects")]
    [SerializeField] private Transform          _cardObj;
    [SerializeField] private Slider             _showTime;
    [SerializeField] private Image              _showComboGauge;

    [Space(10)]
    [Header("▼ Text Objects")]
    [SerializeField] private TextMeshProUGUI    _showFindCardNumber;
    [SerializeField] private TextMeshProUGUI    _showDifficulty;
    [SerializeField] private TextMeshProUGUI    _showRemainPreviewTime;
    [SerializeField] private TextMeshProUGUI    _showGameTimeInfo;

    [Space(10)]
    [Header("▼ Managers")]
    public Transform                            _objectManager;
    public ScreenManager                        _screenManager;
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
#if UNITY_EDITOR
            print("게임 매니저가 존재합니다.");
#endif
            Destroy(gameObject);
        }

        _cards = new List<CardInfo>(new CardInfo[_maxCardTypeCount]);

        int number = 1;

        for (int index = 0; index < _cards.Count; ++index)
        {
            _cards[index] = new CardInfo();
            _cards[index].Number = number++;
        }

        _newCards = new List<CardInfo>();
        _findCard = new CardInfo();
        _curCards = new List<Card>();

        _cardLayoutGroup = _cardObj.gameObject.GetComponent<GridLayoutGroup>();
    }

    private void Start()
    {
        SetResolution();
    }

    private void Update()
    {
        // 피버
        Fever();

        // 미리보기
        Preview();

        // 게임 진행
        Game();
    }


    // Update 관련 함수
    void Fever()
    {
        if (!_isFever) return;

        _feverTime += Time.deltaTime;

        // 피버 게이지 애니메이션
        _showComboGauge.fillAmount = 1 - (_feverTime / _maxFeverTime);

        if (_feverTime >= _maxFeverTime)
        {
            // 피버 타임 끝
            _isFever = false;
            _feverTime = 0;
            _showComboGauge.fillAmount = 0;

            foreach (Card curCard in _curCards)
            {
                curCard.FlipCard(false);
            }
        }
    }

    void Preview()
    {
        if (!_isPreview) return;

        // 미리보기 중일 경우
        // 미리보기 시간 증가
        _previewTime += Time.deltaTime;

        _showRemainPreviewTime.text = ((int)(_maxPreviewTime - _previewTime)).ToString();

        if (_previewTime >= _maxPreviewTime)
        {
            // 미리보기 시간이 끝났을 경우
            PreviewOver();
        }
    }

    void Game()
    {
        if (!_isGame) return;

        _gameTime += Time.deltaTime;

        _showTime.value = 1 - (_gameTime / _maxGameTime);

        if (_gameTime >= _maxGameTime)
        {
            // 게임 끝
            GameOver();
        }
    }

    // User Function
    void GameInit()
    {
        _gameTime = 0;
        _isGame = true;
        Time.timeScale = 1;
        _showTime.value = 1;
        _showComboGauge.fillAmount = 0;
    }

    void StartFever()
    {
        _isFever = true;
        _feverTime = 0;
        // 피버 시간 동안 맞춘 카드를 제외한 모든 카드를 보여준다.
        foreach (Card curCard in _curCards)
        {
            curCard.FlipCard(true);
        }
    }

    void ReturnCards()
    {
        // 카드 반납
        int cardObjChild = _cardObj.childCount;

        for (int index = 0; index < cardObjChild; ++index)
        {
            _cardObj.GetChild(0).SetParent(_objectManager);
        }
    }

    void ShuffleCards()
    {
        if (_cardObj.childCount != 0) ReturnCards();

        // 랜덤 섞기
        int random1, random2;
        CardInfo temp;

        for (int index = 0; index < _cards.Count; ++index)
        {
            random1 = UnityEngine.Random.Range(0, _cards.Count);
            random2 = UnityEngine.Random.Range(0, _cards.Count);

            temp = _cards[random1];
            _cards[random1] = _cards[random2];
            _cards[random2] = temp;
        }
    }

    void BringCards()
    {
        // 무작위로 섞인 카드 숫자 중 pow(n, 2)개 만큼 가져오기
        _newCards = _cards.GetRange(0, (int)Mathf.Pow(_gridSize, 2));

        for (int index = 0; index < _newCards.Count; ++index)
        {
            // 카드 오브젝트 가져오기
            _objectManager.GetChild(0).SetParent(_cardObj);

            _newCards[index].PreviewTime = _maxPreviewTime;

            // 가져온 카드 오브젝트의 숫자 설정
            Card cardScript = _cardObj.GetChild(index).GetComponent<Card>();
            cardScript.SetCardInfo(_newCards[index], _flipCardSize);
            cardScript.SetCardNumberSize(_gridSize);
            _curCards.Add(cardScript);
        }
    }

    void ShowPreview()
    {
        Time.timeScale = 0;
        _previewTime = 0;
        _isPreview = true;

        // Preview 화면을 이전에 보이던 화면에 덧씌움
        _screenManager.CoverScreen("Preview");
    }

    void ShowComboGauge()
    {
        _showComboGauge.fillAmount = (1.0f / _maxComboStack) * _comboStack;
    }

    void StackCombo(bool isCombo)
    {
        if (!isCombo)
        {
            // 콤보가 안쌓임 (Easy, Normal이면 콤보가 한 칸 깎이고 Hard면 전부 깎임)
            switch (_difficulty)
            {
                case DIFFICULTY.EASY:
                case DIFFICULTY.NORMAL:
                    _comboStack = (_comboStack - 1 > 0) ? _comboStack - 1 : 0;
                    break;
                case DIFFICULTY.HARD:
                    _comboStack = 0;
                    break;
            }
        }
        else
        {
            // 콤보가 쌓임
            _comboStack = (_comboStack + 1 > _maxComboStack) ? _comboStack : _comboStack + 1;
            if (_comboStack == _maxComboStack && _isFever == false)
            {
                // 피버 타임
                StartFever();
            }
        }

        ShowComboGauge();
    }

    // public 함수
    public void PreviewOver()
    {
        if (!_isPreview) return; // 이미 종료된 상태이면 작동하지 않는다.

        // 미리보기 종료 후 바로 게임 시작
        _isPreview = false;
        // 미리보기 화면이 제일 나중에 뜬 화면이므로 화면을 제거한다.
        _screenManager.PrevScreen();
        // 카드를 전부 뒤집는다. (카드를 전부 안 보이도록 바꾼다.)
        foreach (Card curCard in _curCards)
        {
            //curCard.FlipCard(false);
            curCard.PreviewOver();
        }

        GameStart();
    }

    public void SelectDifficulty(int gridSize)
    {
        int cellSize = 0;
        int cellSpacing = 0;
        switch ((DIFFICULTY)gridSize)
        {
            case DIFFICULTY.EASY:
                // 10초
                _maxPreviewTime = 10;
                _showDifficulty.text = "쉬움";
                cellSize = 300;
                cellSpacing = 100;
                break;
            case DIFFICULTY.NORMAL:
                // 20초
                _maxPreviewTime = 20;
                _showDifficulty.text = "보통";
                cellSize = 250;
                cellSpacing = 50;
                break;
            case DIFFICULTY.HARD:
                // 30초
                _showDifficulty.text = "어려움";
                _maxPreviewTime = 30;
                cellSize = 200;
                cellSpacing = 50;
                break;
        }
        _difficulty = (DIFFICULTY)gridSize;
        _gridSize = gridSize;
        _maxFeverTime = _maxPreviewTime / 5;
        // 그리드 사이즈 조절
        _flipCardSize = cellSize * 0.8f;
        _cardLayoutGroup.cellSize = new Vector2(_flipCardSize, cellSize);
        _cardLayoutGroup.spacing = new Vector2(cellSpacing, cellSpacing);
    }

    public void GameStart()
    {
        // 미리보기가 끝난 후 미리보기 화면을 끄고 제대로 게임을 시작한다.
        GameInit();
    }

    public void GameReady()
    {
        // 카드 배치, 미리보기 보여주기

        /* 카드 배치 시작 */
        // 그리드 사이즈에 맞게 카드 사이즈 조절
        //int cellSize = -50 * _gridSize + 400;
        //_cardLayoutGroup.cellSize = new Vector2(cellSize, cellSize + 50);

        /*
         * 게임 시작 시 카드 종류가 저장되어 있는 리스트 중 25(pow(n))개를 골라 가져온다.
        */
        // 카드 섞기
        ShuffleCards();

        // 카드 가져오기
        BringCards();

        // 찾아야 하는 카드 뽑기
        ChangeNumber();
        /* 카드 배치 종료 */

        // 미리보기 화면 보여주기
        ShowPreview();
    }

    public void GameClear()
    {
        // 모든 카드를 다 찾았을 때
        _showComboGauge.fillAmount = 0;
        _showTime.value = 0;

        // 게임 오버 화면으로
        _screenManager.GoScreen("GameOver");

        foreach(Card curCard in _curCards)
        {
            curCard.Init();
        }

        // 걸린 시간 보여주기
        _showGameTimeInfo.text = "걸린 시간: " + _gameTime.ToString("F2");
        GameOver(true);
    }

    public void GameOver(bool isClear = false)
    {
        // 변수 초기화
        _comboStack = 0;
        _feverTime = 0;
        _isFever = false;
        _isGame = false;
        _isPreview = false;
        _previewTime = 0;
        _curCards.Clear();

        // 카드 반납
        ReturnCards();

        // 화면 초기화 -> 게임 오버 화면으로
        if (!isClear) _screenManager.ScreenClear();
    }

    public void ChangeNumber()
    {
        // 게임 클리어
        if (_newCards.Count <= 0)
        {
            // 만약 새 카드가 없으면 카드를 섞어서 새롭게 가져온다. -> X
            // 만약 새 카드가 없으면 게임을 종료한다.
            GameClear();
            return;
        }

        int findNumberIndex = Random.Range(0, _newCards.Count);
        _findCard = _newCards[findNumberIndex];
        _showFindCardNumber.text = _findCard.Number.ToString();
    }

    public bool CheckNumber(CardInfo cardInfo)
    {
        if (cardInfo.Number == _findCard.Number)
        {
            _newCards.Remove(_findCard);
            ChangeNumber();
            StackCombo(true);
            return true;
        }
        else
        {
#if UNITY_EDITOR
            print("오답입니다.");
#endif
            StackCombo(false);
            return false;
        }
    }

    public void GameExit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();  
        #endif
    }

    public void IsGamePause(bool isPause)
    {
        Time.timeScale = isPause == true ? 0 : 1;
        _isGame = !isPause;
    }

    /* 해상도 설정하는 함수 */
    public void SetResolution()
    {
        int setWidth = 1080; // 사용자 설정 너비
        int setHeight = 1920; // 사용자 설정 높이

        int deviceWidth = Screen.width; // 기기 너비 저장
        int deviceHeight = Screen.height; // 기기 높이 저장

        Screen.SetResolution(setWidth, (int)(((float)deviceHeight / deviceWidth) * setWidth), true); // SetResolution 함수 제대로 사용하기

        if ((float)setWidth / setHeight < (float)deviceWidth / deviceHeight) // 기기의 해상도 비가 더 큰 경우
        {
            float newWidth = ((float)setWidth / setHeight) / ((float)deviceWidth / deviceHeight); // 새로운 너비
            Camera.main.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f); // 새로운 Rect 적용
        }
        else // 게임의 해상도 비가 더 큰 경우
        {
            float newHeight = ((float)deviceWidth / deviceHeight) / ((float)setWidth / setHeight); // 새로운 높이
            Camera.main.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight); // 새로운 Rect 적용
        }
    }
}

/*
 * 커밋 내역
 *  2023-03-05 00:30 -> 랜덤 숫자 뽑기 완성
 *  2023-03-05 18:21 -> 난이도 설정 및 화면 설정
 *  2023-03-06 19:34 -> 게임 시간 설정 및 미리보기 기능 제작
 *  2023-03-06 20:43 -> 게임 오버 화면 추가 및 콤보 기능 추가
 *  2023-03-07 12:20 -> 피버 기능 추가 (피버 시간은 미리보기 시간의 1/5배)
 *  2023-03-07 15:10 -> 난이도 설정 기능 변경
 *  2023-03-07 22:56 -> UI 어긋난 것 수정
 *  2023-03-15 15:01 ->
     *  셀 사이즈 0.8로 고정
     *  카드 사이즈 조금 더 크게 변경
     *  화면 비율 맞추는 코드 추가
     *  카드 애니메이션 추가
 *  2023-03-15 16:49 -> 피버 애니메이션 수정, 정답 카드 이미지 추가
 *  2023-03-16 18:16 -> 시계 아이콘 및 애니메이션 추가, 시간 조정 추가

 * 변경 내역
 

 * TODO
 *  꾸미기
 *  난이도 선택 부분 꾸미기
*/