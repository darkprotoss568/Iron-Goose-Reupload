using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScript : MonoBehaviour
{
    private WorldScript _worldScript; public WorldScript WorldScript { get { return _worldScript; } set { _worldScript = value; } }
    public WorldScript WS { get { return _worldScript; } }

    private AudioClip _station;
    private AudioClip _attract;
    private AudioClip _gameOverSting;
    private AudioClip _outOfStationSting;

    private bool _bFaded = false;

    private AudioClip _attractLoop;
    private AudioClip _menuMusic;
    private AudioClip _hecticMusic;



    private AudioSource _MusicAudioSource; public AudioSource MusicAudioSource { get { return _MusicAudioSource; } }

    private bool _bCurrentTrackLockedUntilEnd; // Prevent dynamic music changes until the clip has finished playing
    private bool _bCurrentTrackPlayOnce;
    private bool _bCurrentTrackJustBegun; // Force the track not to change until it has started playing

    private float _randomTime1;
    private float _randomTime1_curr;

    private bool _bTrainPullingOutOfStation; public bool BTrainPullingOutOfStation { get { return _bTrainPullingOutOfStation; } set { _bTrainPullingOutOfStation = value; } }

    private float _defaultVolume = 0.4f;

    private bool _bIsPlayingMusic = true; public bool BIsPlayingMusic { get { return _bIsPlayingMusic; } set { _bIsPlayingMusic = value; } }

    void Start()
    {
        _attractLoop = Resources.Load("Music/Music/Attract_Loop") as AudioClip; // main game music
        _menuMusic = Resources.Load("Music/Music/Train_In_Motion_Slower") as AudioClip;   // music for main menu
        _hecticMusic = Resources.Load("Music/Music/Theme_Hectic") as AudioClip; // intense in game music

        _station = Resources.Load("Music/Station_Music_Bed_Loop") as AudioClip;
        _gameOverSting = Resources.Load("Music/Game_over_Sting") as AudioClip;
        _outOfStationSting = Resources.Load("Music/Out_of_station_sting") as AudioClip;

        _bCurrentTrackLockedUntilEnd = false;
        _bCurrentTrackPlayOnce = false;
        _bCurrentTrackJustBegun = false;

        _randomTime1 = 1.0f;
        _randomTime1_curr = 0.0f;

        _MusicAudioSource = GameObject.Find("MainHolder").transform.Find("MusicAudioSource").GetComponent<AudioSource>();
        if (_MusicAudioSource == null) print("Error: _MusicAudioSource == null -- MusicScript @ Start()");
        else if (_MusicAudioSource.isActiveAndEnabled)
        {
            _MusicAudioSource.clip = _attractLoop;
            //_MusicAudioSource.playOnAwake = true;
            _MusicAudioSource.loop = false; // We take care of this manually
            _MusicAudioSource.volume = _defaultVolume;

            if (!_MusicAudioSource.isPlaying)
            {
                _MusicAudioSource.Play();
            }
        }
        else
        {
            print("Error: _MusicAudioSource.isActiveAndEnabled == false -- MusicScript @ Start()");
        }

        _bTrainPullingOutOfStation = false;
    }

    void Update()
    {
        if ((_MusicAudioSource == null) || (_MusicAudioSource.isActiveAndEnabled == false)) return;

        if (!_bIsPlayingMusic)
        {
            if (_MusicAudioSource.isPlaying) _MusicAudioSource.Stop();
            return;
        }
        else
        {
            if (!_MusicAudioSource.isPlaying) _MusicAudioSource.Play();
        }

        /// [Mike, 28-7-18]

        MusicSimplified_Type1();
        return;
    }

    void MusicSimplified_Type1()
    {
        //make music loop if it isnt
        if (_MusicAudioSource.loop == false) _MusicAudioSource.loop = true;

        // if intense music isn't playing
        if (!_worldScript.GameplayScript._BIntenseMusic)
        {
            if (_MusicAudioSource.clip != _attractLoop)
            {
                Debug.Log("start attract");

                _MusicAudioSource.clip = _attractLoop;
            }
        }
        else // intense music is playing
        {
            if (_MusicAudioSource.clip != _hecticMusic)
            {
                if (!_bFaded)
                {
                    FadeOut();
                }

                if (_bFaded)
                {
                    _MusicAudioSource.clip = _hecticMusic;
                }
            }
            else
            {
                if (_bFaded)
                {
                    FadeIn();
                }
            }
        }
    }

    public void FadeOut()
    {
        _MusicAudioSource.volume -= 0.2f * Time.deltaTime;
        if (_MusicAudioSource.volume <= 0)
        {
            _bFaded = true;
        }
    }

    public void FadeIn()
    {
        _MusicAudioSource.volume += 0.2f * Time.deltaTime;
        if (_MusicAudioSource.volume >= _defaultVolume)
        {
            _MusicAudioSource.volume = _defaultVolume;
            _bFaded = true;
        }
    }
}
