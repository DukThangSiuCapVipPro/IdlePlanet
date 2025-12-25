using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SFXType
{
    Click,
    CardUp,
    CardDown,
    CardDeal,
    CardMerge,
    CardTurn,
    Win
}

public class SoundManager : Singleton<SoundManager>
{
    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Background Music")]
    public List<AudioClip> bgmClips;

    [Header("UI Sound Effects")]
    public AudioClip clickSfx;
    public AudioClip winSfx;

    [Header("Game Sound Effects")]
    public AudioClip cardUpSfx;
    public AudioClip cardDownSfx;
    public AudioClip cardDealSfx;
    public AudioClip cardMergeSfx;
    public AudioClip cardTurnSfx;

    private const string BGM_KEY = "BGM_ENABLED";
    private const string SFX_KEY = "SFX_ENABLED";
    private const string VIBRATE_KEY = "VIBRATE_ENABLED";

    public bool BgmEnabled { get; private set; }
    public bool SfxEnabled { get; private set; }
    public bool VibrateEnabled { get; private set; }

    private Coroutine bgmRoutine;

    protected override void Awake()
    {
        base.Awake();

        BgmEnabled = PlayerPrefs.GetInt(BGM_KEY, 1) == 1;
        SfxEnabled = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        VibrateEnabled = PlayerPrefs.GetInt(VIBRATE_KEY, 1) == 1;

        ApplyBGMState();
    }

    // === TOGGLE METHODS ===
    public void ToggleBGM()
    {
        BgmEnabled = !BgmEnabled;
        PlayerPrefs.SetInt(BGM_KEY, BgmEnabled ? 1 : 0);
        ApplyBGMState();
    }

    public void ToggleSFX()
    {
        SfxEnabled = !SfxEnabled;
        PlayerPrefs.SetInt(SFX_KEY, SfxEnabled ? 1 : 0);
    }

    public void ToggleVibrate()
    {
        VibrateEnabled = !VibrateEnabled;
        PlayerPrefs.SetInt(VIBRATE_KEY, VibrateEnabled ? 1 : 0);
    }

    private void ApplyBGMState()
    {
        if (bgmSource != null)
        {
            if (BgmEnabled)
            {
                if (bgmRoutine == null)
                    bgmRoutine = StartCoroutine(PlayRandomBGM());
            }
            else
            {
                if (bgmRoutine != null)
                {
                    StopCoroutine(bgmRoutine);
                    bgmRoutine = null;
                }
                bgmSource.Stop();
            }
        }
    }

    // === RANDOM BGM LOOP ===
    private IEnumerator PlayRandomBGM()
    {
        while (BgmEnabled && bgmClips.Count > 0)
        {
            AudioClip clip = bgmClips[Random.Range(0, bgmClips.Count)];
            bgmSource.clip = clip;
            bgmSource.Play();

            yield return new WaitForSeconds(clip.length);
        }
    }

    // === PLAY SOUND METHODS ===
    public void PlaySFX(AudioClip clip)
    {
        if (SfxEnabled && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFX(SFXType sfxType)
    {
        if (!SfxEnabled) return;

        AudioClip clip = GetClipForType(sfxType);
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlaySFXOverride(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    private AudioClip GetClipForType(SFXType sfxType)
    {
        switch (sfxType)
        {
            case SFXType.Click:
                return clickSfx;
            case SFXType.CardUp:
                return cardUpSfx;
            case SFXType.CardDown:
                return cardDownSfx;
            case SFXType.CardDeal:
                return cardDealSfx;
            case SFXType.CardMerge:
                return cardMergeSfx;
            case SFXType.CardTurn:
                return cardTurnSfx;
            case SFXType.Win:
                return winSfx;
            default:
                return null;
        }
    }

    // Optional: Trigger vibration
    public void Vibrate(long duration = 100, int amplitude = 255)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (VibrateEnabled)
            VibrationManager.Vibrate(50);
#endif
    }
}
