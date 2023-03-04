using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public static GameManager instance;

    private DIFFICULTY difficulty;
    public DIFFICULTY Difficulty { get { return difficulty; } }

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
    }
}
