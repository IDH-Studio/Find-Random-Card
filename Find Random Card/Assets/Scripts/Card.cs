using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CardInfo
{
    public int Number { get; set; }
    public float PreviewTime { get; set; }
}

public class Card : MonoBehaviour
{
    private CardInfo _cardInfo;
    private Button button;
    private Image buttonImage;
    private TextMeshProUGUI showNumberText;
    private bool _isShow = false;

    private void Awake()
    {
        _cardInfo = new CardInfo();
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        showNumberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetCardNumberSize(int gridSize)
    {
        // grid -> text size
        // 3 -> 130
        // 4 -> 105
        // 5 -> 80
        switch (gridSize)
        {
            case 3:
                showNumberText.fontSize = 100;
                break;
            case 4:
                showNumberText.fontSize = 80;
                break;
            case 5:
                showNumberText.fontSize = 60;
                break;
        }
    }

    public void SetCardInfo(CardInfo cardInfo)
    {
        _cardInfo.Number = cardInfo.Number;
        _cardInfo.PreviewTime = cardInfo.PreviewTime;
        showNumberText.text = cardInfo.Number.ToString();

        button.onClick.AddListener(() =>
        {
            // 이미 보여준 카드가 아니라면 카드 클릭 시 카드를 보여주고 잠시 뒤 사라지게 한다.
            if (!_isShow && _cardInfo.Number != -1 && !GameManager._instance.IsFever)
            {
                StartCoroutine(ShowNumber());
            }

            if (GameManager._instance.CheckNumber(_cardInfo))
            {
                // 정답
                _cardInfo.Number = -1;
                buttonImage.color = new Color(150, 150, 150);
            }
        });
    }

    public void FlipCard(bool isShowNumber)
    {
        // 임시로 텍스트가 안보이도록 설정
        if (isShowNumber && _cardInfo.Number != -1) { showNumberText.text = _cardInfo.Number.ToString(); }
        else { showNumberText.text = ""; }
    }

    IEnumerator ShowNumber()
    {
        showNumberText.text = _cardInfo.Number.ToString();
        _isShow = true;
        yield return new WaitForSeconds(0.8f);
        if (_cardInfo.Number != -1) showNumberText.text = "";
        _isShow = false;
    }
}
