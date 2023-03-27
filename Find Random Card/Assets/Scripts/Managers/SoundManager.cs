using System;
using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public enum AUDIO
{
    BGM,
    EFFECT,
}

public class SoundManager : MonoBehaviour
{
    // Audio Sources
    [SerializeField] private AudioSource[]  _audioSources; // 0: BGM, 1: Effect

    [Space(10)]
    [Header("BGM Audio Clips")]
    [SerializeField] private AudioClip[]    _startMusics;
    [SerializeField] private AudioClip      _tutorialMusic;
    [SerializeField] private AudioClip      _previewMusic;
    [SerializeField] private AudioClip[]    _gameMusics;

    [Space(10)]
    [Header("Effect Audio Clips")]
    [SerializeField] private AudioClip      _findCard;
    [SerializeField] private AudioClip      _buttonPopSound;
    [SerializeField] private AudioClip      _buttonPopSoundDown;
    [SerializeField] private AudioClip      _cardButtonSound;
    [SerializeField] private AudioClip      _incorrectSound;

    [Space(10)]
    [Header("Card AudioSources")]
    [SerializeField] private AudioSource[]  _cardAudioSources;

    private Dictionary<string, AudioClip>   _audios;

    // Getter
    public AudioSource[] AudioSources { get { return _audioSources; } }
    public float BGMVolume { get { return _audioSources[(int)AUDIO.BGM].volume; } }
    public float EffectVolume { get { return _audioSources[(int)AUDIO.EFFECT].volume; } }

    private void Awake()
    {
        _audios = new Dictionary<string, AudioClip>();

        _audios.Add("FindCard", _findCard);
        _audios.Add("ButtonPopSound", _buttonPopSound);
        _audios.Add("ButtonPopSoundDown", _buttonPopSoundDown);
        _audios.Add("CardButton", _cardButtonSound);
        _audios.Add("Incorrect", _incorrectSound);

        AudioClip startMusic = _startMusics[Random.Range(0, _startMusics.Length)];
        _audios.Add("StartMusic", startMusic);
        _audios.Add("TutorialMusic", _tutorialMusic);
        _audios.Add("PreviewMusic", _previewMusic);
        
    }

    void RandomGameMusic()
    {
        AudioClip gameMusic = _gameMusics[Random.Range(0, _gameMusics.Length)];
        _audios["GameMusic"] = gameMusic;
    }

    /// <summary>
    /// 효과음 종류: FindCard, ButtonPopSound, ButtonPopSoundDown, CardButton, Incorrect <br></br>
    /// 배경음 종류: StartMusic, TutorialMusic, PreviewMusic, GameMusic
    /// </summary>
    /// <param name="isEffect">효과음이면 true, 배경음이면 false를 입력하세요</param>
    /// <param name="sound">재생할 효과음 또는 배경음의 이름을 입력하세요</param>
    public void Play(bool isEffect, string sound, float pitch=1.0f)
    {
        if (isEffect)
        {
            _audioSources[(int)AUDIO.EFFECT].clip = _audios[sound];
            _audioSources[(int)AUDIO.EFFECT].pitch = pitch;
            _audioSources[(int)AUDIO.EFFECT].Play();
        }
        else
        {
            if (sound == "GameMusic") { RandomGameMusic(); }

            _audioSources[(int)AUDIO.BGM].clip = _audios[sound];
            _audioSources[(int)AUDIO.EFFECT].pitch = pitch;
            _audioSources[(int)AUDIO.BGM].Play();
        }
    }

    /// <summary>
    /// 효과음만 재생 함
    /// </summary>
    /// <param name="sound">종류: FindCard, ButtonPopSound, ButtonPopSoundDown, CardButton, Incorrect</param>
    public void PlayEffectSound(string sound)
    {
        _audioSources[(int)AUDIO.EFFECT].clip = _audios[sound];
        _audioSources[(int)AUDIO.EFFECT].pitch = 1.0f;
        _audioSources[(int)AUDIO.EFFECT].Play();
    }

    /// <summary>
    /// 배경음 또는 효과음의 볼륨을 조절한다
    /// </summary>
    /// <param name="audioType">소리 종류: BGM, EFFECT</param>
    public void ChangeVolume(AUDIO audioType, float volume)
    {
        _audioSources[(int)audioType].volume = volume;

        if (audioType == AUDIO.EFFECT)
        {
            foreach (AudioSource cardAudioSource in _cardAudioSources)
            {
                cardAudioSource.volume = volume;
            }
        }
    }

    public void AudioSetting(AudioData audioData)
    {
        _audioSources[(int)AUDIO.BGM].volume = audioData.BGMVolume;
        _audioSources[(int)AUDIO.EFFECT].volume = audioData.EffectVolume;
    }
}
