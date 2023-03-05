using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
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

public enum DIFFICULTY
{
    EASY,
    NORMAL,
    HARD,
    NONE
}

/*
 * TODO
 * 게임 기능 제작 (프로토타입)
    ㄴ 총 카드 종류 : 30종류
 * 연결 할거 연결하고
 * 꾸밀꺼 꾸미고
 * 추가할거 추가하고
*/

public class CardInfo
{
    public int Number { get; set; }
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private DIFFICULTY _difficulty;
    public DIFFICULTY Difficulty { get { return _difficulty; } }

    private List<CardInfo> cards;
    private List<CardInfo> newCards;

    private CardInfo findCard;

    private GridLayoutGroup _cardLayoutGroup;
    private int _gridSize;

    // Inspector
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameScreen;
    [SerializeField] private TextMeshProUGUI findCardNumber;
    [SerializeField] private Transform cardObj;
    [SerializeField] private TextMeshProUGUI showDifficulty;

    [Space(10)]
    [Header("▼ Managers")]
    public Transform objectManager;
    public ScreenManager screenManager;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            print("게임 매니저가 존재합니다.");
            Destroy(gameObject);
        }

        cards = new List<CardInfo>(new CardInfo[30]);

        int number = 1;

        for (int index = 0; index < cards.Count; ++index)
        {
            cards[index] = new CardInfo();
            cards[index].Number = number++;
        }

        newCards = new List<CardInfo>();
        findCard = new CardInfo();

        _cardLayoutGroup = cardObj.gameObject.GetComponent<GridLayoutGroup>();
    }

    void GameInit()
    {
        //startScreen.SetActive(false);
        //gameScreen.SetActive(true);
        //newCards = Enumerable.Repeat(new CardInfo(), 25).ToList();
    }

    public void SelectDifficulty(int gridSize)
    {
        switch (gridSize)
        {
            case 3:
                _difficulty = DIFFICULTY.EASY;
                showDifficulty.text = "Easy";
                break;
            case 4:
                _difficulty = DIFFICULTY.NORMAL;
                showDifficulty.text = "Normal";
                break;
            case 5:
                _difficulty = DIFFICULTY.HARD;
                showDifficulty.text = "Hard";
                break;
            default:
                _difficulty = DIFFICULTY.NONE;
                showDifficulty.text = "???";
                print("난이도를 설정해야 합니다.");
                break;
        }
        _gridSize = gridSize;
    }

    public void GameStart()
    {
        print("게임 시작");
        //GameInit();

        /*
         * 게임 시작 시 카드 종류가 저장되어 있는 리스트 중 25(pow(n))개를 골라 가져온다.
        */
        // 랜덤 섞기
        int random1, random2;
        CardInfo temp;

        for (int index = 0; index < cards.Count; ++index)
        {
            random1 = UnityEngine.Random.Range(0, cards.Count);
            random2 = UnityEngine.Random.Range(0, cards.Count);

            temp = cards[random1];
            cards[random1] = cards[random2];
            cards[random2] = temp;
        }

        // 무작위로 섞인 카드 숫자 중 pow(n, 2)개 만큼 가져오기
        int cellSize = -50 * _gridSize + 400;
        _cardLayoutGroup.cellSize = new Vector2(cellSize, cellSize + 50);
        newCards = cards.GetRange(0, (int)Mathf.Pow(_gridSize, 2));

        for (int index = 0; index < newCards.Count; ++index)
        {
            // 카드 오브젝트 가져오기
            objectManager.GetChild(0).SetParent(cardObj);
            // 가져온 카드 오브젝트의 숫자 설정
            cardObj.GetChild(index).GetComponent<Card>().SetCardInfo(newCards[index]);
        }

        ChangeNumber();
    }

    public void GameOver()
    {
        screenManager.ScreenClear();
        int cardObjChild = cardObj.childCount;

        for (int index = 0; index < cardObjChild; ++index)
        {
            cardObj.GetChild(0).SetParent(objectManager);
        }
    }

    public void ChangeNumber()
    {
        if (newCards.Count <= 0)
        {
            GameOver();
            return;
        }

        int findNumberIndex = Random.Range(0, newCards.Count);
        findCard = newCards[findNumberIndex];
        findCardNumber.text = findCard.Number.ToString();
    }

    public bool CheckNumber(CardInfo cardInfo)
    {
        if (cardInfo.Number == findCard.Number)
        {
            print("정답을 맞추셨습니다!");
            newCards.Remove(findCard);
            ChangeNumber();
            return true;
        }
        else
        {
            print("오답입니다.");
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
}

/*
 * 2023-03-05 00:30 -> 랜덤 숫자 뽑기 완성
 * 2023-03-05 18:21 -> 난이도 설정 및 화면 설정
 * TODO
 *  그리드 숫자에 따라 뽑는 숫자 및 카드 보이는거 달리 하기
*/