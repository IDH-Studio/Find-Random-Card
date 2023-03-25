using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardInfo
{
    public int Number { get; set; }
    public float PreviewTime { get; set; }
    public bool isCorrect { get; set; } = false;
}

public class Card : MonoBehaviour
{
    [SerializeField] private Sprite _cardBack;
    [SerializeField] private Sprite _cardFront;
    [SerializeField] private Sprite _cardFrontCorrect;

    [Space(10)]
    [SerializeField] private float _showAnimation = 0.7f;
    [SerializeField] private float _speedTimes = 1;

    [Space(10)]
    [SerializeField] private Animator _flipAnim;

    private CardInfo _cardInfo;
    private Button _button;
    private Image _buttonImage;
    private TextMeshProUGUI _showNumberText;
    private ParticleSystem _particle;
    private AudioSource _audio;
    private bool _isShow = false;

    // 애니메이션 이름
    private string _cardFlipAnim = "Card_Flip";
    private string _ReverseCardFlipAnim = "Card_Flip_Reverse";

    private void Awake()
    {
        _cardInfo = new CardInfo();
        _cardInfo.isCorrect = false;
        _button = GetComponent<Button>();
        _buttonImage = GetComponent<Image>();
        _showNumberText = GetComponentInChildren<TextMeshProUGUI>();
        _particle = GetComponent<ParticleSystem>();
        _audio = GetComponent<AudioSource>();
    }


    void PlayAnimation(string animationName, bool isPlay = true)
    {
        _buttonImage.color = new Color(0, 0, 0, 0);
        _showNumberText.text = "";
        _flipAnim.SetBool(animationName, isPlay);
        StartCoroutine(Stop(animationName, 0.7f / _speedTimes));
    }

    IEnumerator Stop(string animationName, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        _flipAnim.SetBool(animationName, false);
        if (animationName == _cardFlipAnim)
        {
            _showNumberText.text = _cardInfo.Number.ToString();
            _buttonImage.sprite = _cardInfo.isCorrect == true ? _cardFrontCorrect : _cardFront;
        }
        else if (animationName == _ReverseCardFlipAnim)
        {
            _showNumberText.text = "";
            _buttonImage.sprite = _cardBack;

        }
        _buttonImage.color = new Color(255, 255, 255, 255);
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
                _showNumberText.fontSize = 100;
                break;
            case 4:
                _showNumberText.fontSize = 80;
                break;
            case 5:
                _showNumberText.fontSize = 60;
                break;
        }
    }

    public void SetCardInfo(CardInfo cardInfo, float flipCardSize)
    {
        _buttonImage.sprite = _cardFront;
        _cardInfo.Number = cardInfo.Number;
        _cardInfo.PreviewTime = cardInfo.PreviewTime;
        _cardInfo.isCorrect = false;
        _showNumberText.text = cardInfo.Number.ToString();
        _flipAnim.transform.localScale = new Vector2(flipCardSize, flipCardSize);

        _button.onClick.AddListener(() =>
        {
            if (_flipAnim.GetBool(_cardFlipAnim) || _flipAnim.GetBool(_ReverseCardFlipAnim)) return;

            // 이미 보여준 카드가 아니라면 카드 클릭 시 카드를 보여주고 잠시 뒤 사라지게 한다.
            if (!_isShow && _cardInfo.isCorrect == false && !GameManager._instance.IsFever)
            {
                StartCoroutine(ShowNumber());
                _audio.Play();
            }

            if (GameManager._instance.CheckNumber(_cardInfo))
            {
                // 정답
                _cardInfo.isCorrect = true;
                _buttonImage.sprite = _cardFrontCorrect;
                _particle.Play();
                GameManager._instance._soundManager.Play(true, "FindCard", 1.05f);
            }
        });
    }

    public void PreviewOver()
    {
        _showNumberText.text = "";
        PlayAnimation(_ReverseCardFlipAnim);
    }

    public void Init()
    {
        _flipAnim.SetBool(_cardFlipAnim, false);
        _flipAnim.SetBool(_ReverseCardFlipAnim, false);
        _cardInfo.isCorrect = false;
        _isShow = false;
        _buttonImage.sprite = _cardFront;
        _buttonImage.color = new Color(255, 255, 255, 255);
        _button.onClick.RemoveAllListeners();
        StopAllCoroutines();
    }

    public void FlipCard(bool isShowNumber)
    {
        if (_cardInfo.isCorrect) return;

        // 카드 뒷면 -> 앞면이 기본 애니메이션
        // 카드 앞면 -> 뒷면이 역 애니메이션
        string animationName = isShowNumber == true ? _cardFlipAnim : _ReverseCardFlipAnim;

        PlayAnimation(animationName);
    }

    IEnumerator ShowNumber()
    {
        // 카드 뒷면 -> 앞면
        PlayAnimation(_cardFlipAnim);
        _isShow = true;

        yield return new WaitForSeconds(_showAnimation / _speedTimes);

        if (_cardInfo.isCorrect == false)
        {
            // 카드 앞면 -> 뒷면
            PlayAnimation(_ReverseCardFlipAnim);
        }
        _isShow = false;
    }
}
