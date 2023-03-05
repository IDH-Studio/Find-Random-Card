using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    private CardInfo _cardInfo;
    private Button button;
    private TextMeshProUGUI numberText;

    private void Awake()
    {
        _cardInfo = new CardInfo();
        button = GetComponent<Button>();
        numberText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetCardInfo(CardInfo cardInfo)
    {
        _cardInfo.Number = cardInfo.Number;
        numberText.text = cardInfo.Number.ToString();

        button.onClick.AddListener(() =>
        {
            if(GameManager.instance.CheckNumber(_cardInfo))
            {
                // Á¤´ä
                _cardInfo.Number = -1;
                numberText.text = "";
            }
        });
    }
}
