using PopupSystem;
using ThangDD;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PopupSettings : SingletonPopup<PopupSettings>
{
    [Header("Action Buttons")]
    [SerializeField] private Button btnHome;
    [SerializeField] private Button btnRestart;

    [Header("Toggle Buttons with Sprites")]
    [SerializeField] private Button btnBgm;
    [SerializeField] private Image imgBgm;
    [SerializeField] private Sprite spriteBgmOn;
    [SerializeField] private Sprite spriteBgmOff;

    [SerializeField] private Button btnSfx;
    [SerializeField] private Image imgSfx;
    [SerializeField] private Sprite spriteSfxOn;
    [SerializeField] private Sprite spriteSfxOff;

    [SerializeField] private Button btnVibrate;
    [SerializeField] private Image imgVibrate;
    [SerializeField] private Sprite spriteVibrateOn;
    [SerializeField] private Sprite spriteVibrateOff;

    public RectTransform rectContent;

    private void Start()
    {
        // Add button listeners
        if (btnHome != null)
            btnHome.onClick.AddListener(OnHomeClicked);

        if (btnRestart != null)
            btnRestart.onClick.AddListener(OnRestartClicked);

        if (btnBgm != null)
            btnBgm.onClick.AddListener(OnBgmClicked);

        if (btnSfx != null)
            btnSfx.onClick.AddListener(OnSfxClicked);

        if (btnVibrate != null)
            btnVibrate.onClick.AddListener(OnVibrateClicked);

        // Initialize toggle button states
        UpdateToggleStates();
    }

    public void Show()
    {
        base.Show();
        UpdateToggleStates();
    }

    /// <summary>
    /// Update all toggle button sprites based on current SoundManager states
    /// </summary>
    private void UpdateToggleStates()
    {
        bool isSceneGame = true;
        btnHome.gameObject.SetActive(isSceneGame);
        btnRestart.gameObject.SetActive(isSceneGame);
        // btnOpenCheat.enabled = GlobalSettings.Instance.IsCheatEnabled;

        Utils.DelayFrame(() =>
        {
            var rect = GetComponent<RectTransform>();
            if (isSceneGame)
                rect.sizeDelta = new Vector2(700, rectContent.sizeDelta.y + 160);
            else
                rect.sizeDelta = new Vector2(700, rectContent.sizeDelta.y + 320);
        });

        if (SoundManager.Instance == null) return;

        // Update BGM sprite
        if (imgBgm != null)
            imgBgm.sprite = SoundManager.Instance.BgmEnabled ? spriteBgmOn : spriteBgmOff;

        // Update SFX sprite
        if (imgSfx != null)
            imgSfx.sprite = SoundManager.Instance.SfxEnabled ? spriteSfxOn : spriteSfxOff;

        // Update Vibrate sprite
        if (imgVibrate != null)
            imgVibrate.sprite = SoundManager.Instance.VibrateEnabled ? spriteVibrateOn : spriteVibrateOff;
    }

    // === ACTION BUTTONS ===

    private void OnHomeClicked()
    {
        
    }

    private void OnRestartClicked()
    {
        
    }

    // === TOGGLE BUTTONS ===

    private void OnBgmClicked()
    {
        SoundManager.Instance.ToggleBGM();
        if (imgBgm != null)
            imgBgm.sprite = SoundManager.Instance.BgmEnabled ? spriteBgmOn : spriteBgmOff;
    }

    private void OnSfxClicked()
    {
        SoundManager.Instance.ToggleSFX();
        if (imgSfx != null)
            imgSfx.sprite = SoundManager.Instance.SfxEnabled ? spriteSfxOn : spriteSfxOff;
    }

    private void OnVibrateClicked()
    {
        SoundManager.Instance.ToggleVibrate();
        if (imgVibrate != null)
            imgVibrate.sprite = SoundManager.Instance.VibrateEnabled ? spriteVibrateOn : spriteVibrateOff;
    }
}
