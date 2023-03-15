using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card_Test : MonoBehaviour
{
    [SerializeField] private Sprite _cardBack;
    [SerializeField] private Sprite _cardFront;

    [Space(10)]
    [SerializeField] private Animator _flipAnim;

    private Button _button;
    private Image _buttonImage;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            print("Å¬¸¯");
            Flip();
        });
        _buttonImage = GetComponent<Image>();

        //_flipAnim = GetComponent<Animator>();
    }

    void Flip()
    {
        _buttonImage.sprite = null;
        _buttonImage.color = new Color(0, 0, 0, 0);
        _flipAnim.SetBool("Card_Flip", true);
        StartCoroutine(StopAnimation("Card_Flip", 1f));
    }

    IEnumerator StopAnimation(string animationName, float delay)
    {
        yield return new WaitForSeconds(delay);
        _buttonImage.sprite = _cardBack;
        _buttonImage.color = new Color(255, 255, 255, 255);
        _flipAnim.SetBool(animationName, false);
    }
}
