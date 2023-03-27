using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using TMPro.Examples;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    // 난이도 관련 변수
    private DIFFICULTY                          _difficulty;
    private string[]                            _difficultyTypes = { "쉬움", "보통", "어려움" };
    private float[]                             _difficultyTimes = { 60f, 75f, 90f };

    // 게임 관련 변수
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
    private sbyte                               _comboStatus = 0;
    [SerializeField] private float              _maxComboAnimationTime = 0.5f;
    private float                               _comboAnimationTime = 0f;
    private float                               _comboPart;
    private float                               _comboStackPoint; // _comboPart * _comboStack
    private float                               _startValue;    // 콤보 애니메이션에 필요한 변수
    private float                               _endValue;      // 콤보 애니메이션에 필요한 변수


    // Getter, Setter
    public DIFFICULTY                           Difficulty { get { return _difficulty; } }
    public bool                                 IsFever { get { return _isFever; } }
    public float                                GameTime { get { return _gameTime; } }


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
    [SerializeField] private Image              _showTimeImage;
    [SerializeField] private Image              _showComboGauge;
    [SerializeField] private TMP_InputField     _nickname;

    [Space(10)]
    [Header("▼ Text Objects")]
    [SerializeField] private TextMeshProUGUI    _showFindCardNumber;
    [SerializeField] private TextMeshProUGUI    _showDifficulty;
    [SerializeField] private TextMeshProUGUI    _showRemainPreviewTime;
    [SerializeField] private TextMeshProUGUI    _showGameTimeInfo;
    [SerializeField] private TextMeshProUGUI    _showGameoverDifficulty;

    [Space(10)]
    [Header("▼ Managers")]
    public Transform                            _objectManager;
    public ScreenManager                        _screenManager;
    public SoundManager                         _soundManager;
    public SettingManager                       _settingManager;
    public DatabaseManager                      _databaseManager;
    public PrefabManager                        _prefabManager;
    public AdMobManager                         _adMobManager;
    
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

        _comboPart = 1.0f / _maxComboStack;
        _comboStackPoint = _comboPart * _comboStack;
    }

    private void Start()
    {
        SetResolution();
        _soundManager.Play(false, "StartMusic");
        _adMobManager.LoadAd();
    }

    private void Update()
    {
        // 콤보
        Combo();

        // 피버
        Fever();

        // 미리보기
        Preview();

        // 게임 진행
        Game();
    }

    private void OnDestroy()
    {
        _adMobManager.DestroyAd();
    }

    // Update 관련 함수
    void Combo()
    {
        if (_comboStatus == 0 && !_isFever) return;

        // 경과 시간 업데이트
        _comboAnimationTime += Time.deltaTime;

        // 애니메이션 진행 상태 계산
        float progress = Mathf.Clamp01(_comboAnimationTime / _maxComboAnimationTime);

        // 현재 값 계산
        _startValue = Mathf.Lerp(_startValue, _endValue, progress);

        // 현재 값으로 애니메이션 적용
        _showComboGauge.fillAmount = _startValue;

        // 애니메이션 종료 체크
        if (progress >= 1f)
        {
            _comboStatus = 0;
            _comboAnimationTime = 0;
            return;
        }
    }

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
            _comboStack = 0;

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
        _showTimeImage.fillAmount = _showTime.value;

        if (_gameTime >= _maxGameTime)
        {
            // 게임 끝
            GameOver();
        }
    }

    // User Function

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
        Time.timeScale = 1;
        _previewTime = 0;
        _isPreview = true;

        // Preview 화면을 이전에 보이던 화면에 덧씌움
        _screenManager.CoverScreen("Preview");
    }

    void ShowComboGauge()
    {
        _showComboGauge.fillAmount = _comboPart * _comboStack;
    }

    void StackCombo(bool isCombo)
    {
        _comboStackPoint = _comboPart * _comboStack;
        if (!isCombo)
        {
            // 콤보가 안쌓임 (Easy, Normal이면 콤보가 한 칸 깎이고 Hard면 전부 깎임)
            if (_comboStatus != -1)
            {
                // 만약 오답이 아니었다가 오답인 경우에는 _comboStatus도 변화하고 _startValue도 변화한다.
                _comboStatus = -1;
                _startValue = _comboStackPoint;
            }

            // 만약 이전에도 오답이고 현재도 오답인 경우 _startValue의 변화는 없다.

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

            _endValue = _comboPart * _comboStack;
        }
        else
        {
            // 콤보가 쌓임

            if (_comboStatus != 1)
            {
                // 만약 정답이 아니었다가 정답인 경우에는 _comboStatus도 변화하고 _startValue도 변화한다.
                _comboStatus = 1;
                _startValue = _comboStackPoint;
            }

            _comboStack = (_comboStack + 1 > _maxComboStack) ? _comboStack : _comboStack + 1;
            _endValue = _comboPart * _comboStack;

            if (_comboStack == _maxComboStack && _isFever == false)
            {
                // 피버 타임
                StartFever();
            }
        }

        //ShowComboGauge();
    }

    void GameStart()
    {
        // 난이도에 맞게 게임 세팅
        //GameSetting();

        print("게임 시간: " + _maxGameTime);
        // 미리보기가 끝난 후 미리보기 화면을 끄고 제대로 게임을 시작한다.
        _gameTime = 0;
        _comboStack = 0;
        _feverTime = 0;
        _isFever = false;
        Time.timeScale = 1;
        _showTime.value = 1;
        _showTimeImage.fillAmount = _showTime.value;
        _showComboGauge.fillAmount = 0;

        _isGame = true;
    }

    void Incorrect()
    {
        // 오답일 경우 쉬움, 보통 난이도는 1초
        // 어려움 난이도는 2초가 깎이며 실패 효과음이 들린다.
        switch (_difficulty)
        {
            case DIFFICULTY.EASY:
            case DIFFICULTY.NORMAL:
                _gameTime += 1f;
                break;
            case DIFFICULTY.HARD:
                _gameTime += 2f;
                break;
        }
        _soundManager.PlayEffectSound("Incorrect");
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
        _maxGameTime = _difficultyTimes[gridSize - 3];
        _maxFeverTime = _maxPreviewTime / 5;
        // 그리드 사이즈 조절
        _flipCardSize = cellSize * 0.8f;
        _cardLayoutGroup.cellSize = new Vector2(_flipCardSize, cellSize);
        _cardLayoutGroup.spacing = new Vector2(cellSpacing, cellSpacing);
        _databaseManager.SetDatabase(_difficulty);
        _showGameoverDifficulty.text = "난이도: " + _difficultyTypes[(int)_difficulty - 3];
    }

    // Difficulty Screen에서 게임 시작 버튼을 누를 시 호출
    public void GameReady()
    {
        // 광고 보여주기
        _adMobManager.ShowAd();

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
        GameOver(true);

        // 데이터 가져오기
        _databaseManager.GetDatas();

        // 게임 오버 화면으로
        _screenManager.GoScreen("GameOver");

        // 걸린 시간 보여주기
        _showGameTimeInfo.text = "걸린 시간: " + _gameTime.ToString("F2");
    }

    public void GameOver(bool isClear = false)
    {
        _isGame = false;

        // 광고 숨기기
        _adMobManager.HideAd();

        foreach (Card curCard in _curCards)
        {
            curCard.Init();
        }

        _showComboGauge.fillAmount = 0;
        _showTime.value = 0;
        _showTimeImage.fillAmount = _showTime.value;

        // 변수 초기화
        _comboStatus = 0;
        _isPreview = false;
        _previewTime = 0;
        _comboAnimationTime = 0;
        _startValue = 0;
        _endValue = 0;
        _curCards.Clear();

        // 카드 반납
        ReturnCards();

        // 화면 초기화 -> 게임 오버 화면으로
        if (!isClear) _screenManager.ScreenClear();
        //_screenManager.ScreenClear();
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
            if (!_isFever) StackCombo(true);
            return true;
        }
        else
        {
#if UNITY_EDITOR
            print("오답입니다.");
#endif
            Incorrect();
            if (!_isFever) StackCombo(false);
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

    /* 데이터 저장 함수 */
    public void SaveScore()
    {
        string nickname = _nickname.text;

        if (nickname == "" || nickname.Length > 7)
        {
            return;
        }

        // ½º??¾? ???? ±?´?
        if (_databaseManager.WriteData(nickname, _gameTime))
        {
#if UNITY_EDITOR
            print("데이터 쓰기 성공");
#endif
        }
        else
        {
#if UNITY_EDITOR
            print("데이터 쓰기 실패");
#endif
        }
        _nickname.text = "";
        _screenManager.PrevScreen();

        _databaseManager.GetDatas();
    }

    public void ShowRanking()
    {
        _screenManager.GoScreen("Ranking");
        _databaseManager.GetDatas("easy");
    }

    public void CloseRanking()
    {
        _screenManager.PrevScreen();
        _databaseManager.PutBackScores();
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
 *  2023-03-20 16:54 -> 인게임 시간, 콤보 이미지, 애니메이션 추가, 각종 버그 수정(난이도 버튼 버그, 미리보기 시간 버그, 카드 애니메이션 버그, 콤보 버그 등)
 *  2023-03-25 23:14 -> 이미지, 파티클, 음악, 화면, DB연동 추가
    * 배경화면 추가
    * 정답일 시 파티클 보이게 파티클 추가
    * 카드 이미지 및 애니메이션 변경
    * 배경음, 효과음 추가
    * 설정 화면 추가
    * 게임 오버 화면 수정
    * DB연동
 * 2023-03-25 23:34 -> 광고 추가
 * 2023-03-27 14:18 -> 랭킹 화면 추가
 * 2023-03-27 16:41 -> 난이도별 시간 변경, 오답 기능 추가, 효과음 추가, 정답 애니메이션 추가
    * 난이도별로 게임 시간 다르게 조정
    * 오답 효과음 추가
    * 오답 시 게임 시간 감소 및 효과음 출력 기능 추가
    * 카드 정답 애니메이션 추가
 * 2023-03-27 20:05 -> 튜토리얼 화면 수정

 * 변경 내역

 * TODO
    * 꾸미기
    * 난이도 선택 부분 꾸미기
    * 음악 넣기(배경음, 효과음) -> 필요한 음악 더 있으면 추가해야 함(피버 음악(미정))

 * 진행 중인 작업
    * 튜토리얼 화면 추가
*/


/*
 * 버그 기록
    * 2023-03-20
        * 게임 두판 진행 후 미리보기할 때 카드가 저절로 넘어가며 Fever상태로 돌입됨      O
        * 게임 시작과 동시에 카드를 터치하면 애니메이션이 실행되지 않음                  O
*/


/*
 * 버그 수정 기록
 * 2023-03-20 15:37
    * 애니메이션 실행 후 이미지가 남는 버그가 있을 경우 애니메이션을 실행하는 오브젝트의 SpriteRenderer를 수정하는 것이 아니라
    * 애니메이션의 실행을 중단하면 해결된다.
*/