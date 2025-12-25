using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.InputSystem;

public class HUDManager : Singleton<HUDManager>
{
    protected override bool DontDestroyOnLoadEnabled => false;

    public RectTransform topPanel;

    public RectTransform parentRectTransform;
    public RectTransform flyIconPrefab;

    public Toast toastPrefab;

    //Click Effect
    public GameObject clickEffectPrefab;
    public float dragThreshold = 30f; // pixels

    private Vector2 touchStartPos;
    private bool potentialTap = false;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {

    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        // Mouse input using new Input System
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                touchStartPos = Mouse.current.position.ReadValue();
                potentialTap = true;
            }

            if (Mouse.current.leftButton.isPressed)
            {
                if (Vector2.Distance(touchStartPos, Mouse.current.position.ReadValue()) > dragThreshold)
                    potentialTap = false;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame && potentialTap)
            {
                if (clickEffectPrefab)
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
                    worldPos.z = 0;
                    ObjectPool.Spawn(clickEffectPrefab, worldPos, Quaternion.identity);
                    SoundManager.Instance.PlaySFX(SFXType.Click);
                }
            }
        }
#else
        // Touch input using new Input System
        if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
        {
            var touch = Touchscreen.current.touches[0];

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
            {
                touchStartPos = touch.position.ReadValue();
                potentialTap = true;
            }

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved ||
                touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary)
            {
                if (Vector2.Distance(touchStartPos, touch.position.ReadValue()) > dragThreshold)
                    potentialTap = false;
            }

            if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended && potentialTap)
            {
                if (clickEffectPrefab)
                {
                    Vector3 worldPos = Camera.main.ScreenToWorldPoint(touch.position.ReadValue());
                    worldPos.z = 0;
                    ObjectPool.Spawn(clickEffectPrefab, worldPos, Quaternion.identity);
                    SoundManager.Instance.PlaySFX(SFXType.Click);
                }
                potentialTap = false; // Reset to prevent multiple spawns from the same touch
            }
        }
#endif
    }

    void OnChangeScene()
    {

    }

    public void Init()
    {

    }

    public void ShowHUD(bool show)
    {
        if (show)
        {
            topPanel.DOAnchorPosY(0, 0.5f);
        }
        else
        {
            topPanel.DOAnchorPosY(topPanel.rect.height, 0.5f);
        }
    }

    public void AddResourceEffect(ResourceType type, Transform startPos, int amount, Action callback)
    {
        switch (type)
        {
            default:
                Debug.LogWarning("Unknown resource type: " + type);
                break;
        }
    }

    public void PlayFlyEffect(Vector3 startPos, RectTransform targetUI, SpriteRenderer spriteRenderer, int count = 1, bool fromWorld = true, Action callback = null)
    {
        int completed = 0;

        for (int i = 0; i < count; i++)
        {
            Vector3 screenPos = fromWorld ? Camera.main.WorldToScreenPoint(startPos) : startPos;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform, screenPos, Camera.main, out localPos);

            RectTransform icon = ObjectPool.Spawn(flyIconPrefab, parentRectTransform);
            Image iconImage = icon.GetChild(2).GetComponent<Image>();
            if (iconImage && spriteRenderer.sprite)
                iconImage.sprite = spriteRenderer.sprite;
            icon.transform.localScale = Vector3.one;
            icon.anchoredPosition = localPos;

            // Scale the icon to visually match the SpriteRenderer's on-screen size
            Camera cam = Camera.main;
            Bounds worldBounds = spriteRenderer.bounds;
            Vector3 minScreen = cam.WorldToScreenPoint(worldBounds.min);
            Vector3 maxScreen = cam.WorldToScreenPoint(worldBounds.max);

            Vector2 minLocal;
            Vector2 maxLocal;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, minScreen, cam, out minLocal);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, maxScreen, cam, out maxLocal);

            Vector2 desiredLocalSize = new Vector2(Mathf.Abs(maxLocal.x - minLocal.x), Mathf.Abs(maxLocal.y - minLocal.y));
            Vector2 prefabSize = icon.sizeDelta;
            if (prefabSize.x > 0 && prefabSize.y > 0)
            {
                Vector3 fitScale = new Vector3(
                    desiredLocalSize.x / prefabSize.x,
                    desiredLocalSize.y / prefabSize.y,
                    1f);
                icon.transform.DOScale(fitScale * 1.3f, 0.2f);
            }

            Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(targetUI.position);
            Vector2 targetLocalPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform, targetScreenPos, Camera.main, out targetLocalPos);

            Vector2 randomOffset = new Vector2(Random.Range(-80f, 80f), Random.Range(0f, 60f));
            Vector2 firstHop = localPos + randomOffset;
            icon.DOAnchorPos(firstHop, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                icon.transform.DOScale(0, 1f);
                icon.DOAnchorPos(targetLocalPos, 1f)
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() =>
                    {
                        ObjectPool.DeSpawn(icon);
                        completed++;
                        if (completed == count)
                            callback?.Invoke();
                    });
            });
        }
    }

    public void PlayResourceEffect(Vector3 startPos, RectTransform targetUI, Sprite sprite, int count = 1, bool fromWorld = true, Action callback = null)
    {
        int completed = 0;
        if (count > 10)
            count = 10; // Limit to 10 icons at a time
        for (int i = 0; i < count; i++)
        {
            Vector3 screenPos = fromWorld ? Camera.main.WorldToScreenPoint(startPos) : startPos;

            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform, screenPos, Camera.main, out localPos);

            RectTransform icon = ObjectPool.Spawn(flyIconPrefab, parentRectTransform);
            Image iconImage = icon.GetChild(2).GetComponent<Image>();
            if (iconImage && sprite)
                iconImage.sprite = sprite;

            icon.transform.localScale = Vector3.one;
            icon.anchoredPosition = localPos;

            icon.sizeDelta = new Vector2(50, 50);

            Vector3 targetScreenPos = Camera.main.WorldToScreenPoint(targetUI.position);
            Vector2 targetLocalPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRectTransform, targetScreenPos, Camera.main, out targetLocalPos);

            Vector2 randomOffset = new Vector2(Random.Range(-250 - count * 5, 250 + count * 5), Random.Range(-150 - count * 5, 150 + count * 5));
            Vector2 firstHop = localPos + randomOffset;
            icon.DOAnchorPos(firstHop, 0.5f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                icon.transform.DOScale(0.85f, Random.Range(0.6f, 1f));
                icon.DOAnchorPos(targetLocalPos, Random.Range(0.75f, 1f))
                    .SetEase(Ease.InOutQuad)
                    .OnComplete(() =>
                    {
                        ObjectPool.DeSpawn(icon);
                        completed++;
                        if (completed == count)
                            callback?.Invoke();
                    });
            });
        }
    }

    Toast toast;
    public void ShowToast(string mess, float duration = 2)
    {
        if (toast == null)
            toast = Instantiate(toastPrefab, parentRectTransform);

        toast.SetMessage(mess);
    }
}

public enum ResourceType
{

}