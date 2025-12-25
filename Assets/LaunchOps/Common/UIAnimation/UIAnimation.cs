using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.UI;

public enum AnimationType
{
    Scale,
    Position,
    Fade,
    Rotate,
    PunchScale,
    PunchRotation,
    PunchPosition
}
[Serializable]
public class AnimatedProperties
{
    public AnimationType AnimationType;

   
    public Ease EaseType;

    public int LoopCount = 0;
    public float LoopDelay;
    public LoopType LoopType;

    [Header("Start")]
    [DrawIf("AnimationType", AnimationType.Position)]
    public Vector3 positionStart;
    [DrawIf("AnimationType", AnimationType.Scale)]
    public Vector3 scaleStart;
    [DrawIf("AnimationType", AnimationType.Rotate)]
    public Vector3 startRotation;
    [DrawIf("AnimationType", AnimationType.Fade)]
    public float fadeStart;
    [DrawIf("AnimationType", AnimationType.PunchScale)]
    public Vector3 punchScaleStart;

    [DrawIf("AnimationType", AnimationType.PunchPosition)]
    public Vector3 punchPositionStart;

    [DrawIf("AnimationType", AnimationType.PunchRotation)]
    public Vector3 punchRotationStart;
    [Header("End")]
    [DrawIf("AnimationType", AnimationType.Scale)]
    public Vector3 scaleEnd;
    [DrawIf("AnimationType", AnimationType.Position)]
    public Vector3 positionEnd;
    [DrawIf("AnimationType", AnimationType.Fade)]
    public float fadeEnd;
    [DrawIf("AnimationType", AnimationType.Rotate)]
    public Vector3 endRotation;
    [DrawIf("AnimationType", AnimationType.PunchScale)]
    public Vector3 punchScaleEnd;

    [DrawIf("AnimationType", AnimationType.PunchPosition)]
    public Vector3 punchPositionEnd;

    [DrawIf("AnimationType", AnimationType.PunchRotation)]
    public Vector3 punchRotationEnd;
    [Header("Extra Settings")]

    [DrawIf("AnimationType", AnimationType.Rotate)]
    public bool useShortestRotation = false;

    [DrawIf("AnimationType", AnimationType.PunchScale)]
  
    public float punchScaleElasticity = 1;
    [DrawIf("AnimationType", AnimationType.PunchScale)]

    public int punchScaleVibration = 10;
    [DrawIf("AnimationType", AnimationType.PunchRotation)]

    public float punchRotationElasticity = 1;
    [DrawIf("AnimationType", AnimationType.PunchRotation)]

    public int punchRotationVibration = 10;
    [DrawIf("AnimationType", AnimationType.PunchPosition)]

    public float punchPositionElasticity = 1;
    [DrawIf("AnimationType", AnimationType.PunchPosition)]

    public int punchPositionVibration = 10;
}
[Serializable]
public class PunchSettings
{
  
}
public class UIAnimation : MonoBehaviour
{
    public float Delay;
    
    public float Duration=1f;

    public bool IgnoreTimeScale = false;
    
    public List<AnimatedProperties> Animations = new List<AnimatedProperties>();

    private RectTransform rect;
    public UnityEvent OnStart;
    public UnityEvent OnComplete;
    Sequence sequence;
    private void OnEnable()
    {
        if (sequence.IsActive())
        {
            sequence.Kill();
        }
    
        OnStart?.Invoke();
        if(rect== null)
        {
            rect = this.transform.GetComponent<RectTransform>();
        }

        if (WaitAndTrigger != null)
        {
            StopCoroutine(WaitAndTrigger);
        }
        WaitAndTrigger = iWaitAndTrigger();
        StartCoroutine(WaitAndTrigger);
         sequence = DOTween.Sequence().SetUpdate(IgnoreTimeScale);
        foreach(var anim in Animations)
        {
            Tween tween= null;
            if(anim.AnimationType == AnimationType.PunchScale)
            {
                this.transform.localScale = anim.punchScaleStart;
                tween =  this.transform.DOPunchScale(anim.punchScaleEnd, Duration, anim.punchScaleVibration, anim.punchScaleElasticity).SetEase(anim.EaseType);
            }
            else if (anim.AnimationType == AnimationType.PunchRotation)
            {
                this.transform.rotation = Quaternion.Euler(anim.punchRotationStart);

                tween = this.transform.DOPunchRotation(anim.punchRotationEnd, Duration, anim.punchRotationVibration, anim.punchRotationElasticity).SetEase(anim.EaseType);
            }
            else if (anim.AnimationType == AnimationType.PunchPosition)
            {
                this.transform.rotation = Quaternion.Euler(anim.punchPositionStart);

                tween = this.transform.DOPunchPosition(anim.punchPositionEnd, Duration, anim.punchRotationVibration, anim.punchRotationElasticity).SetEase(anim.EaseType);
            }
            else if(anim.AnimationType == AnimationType.Scale)
            {
                int loopCounter = anim.LoopCount;
                this.transform.localScale = anim.scaleStart;
                tween = this.transform.DOScale(anim.scaleEnd, Duration).SetEase(anim.EaseType);

               
            }
            else if(anim.AnimationType == AnimationType.Rotate)
            {
                this.transform.rotation = Quaternion.Euler(anim.startRotation);
                tween = this.transform.DORotate(anim.endRotation,Duration).SetEase(anim.EaseType).SetOptions(anim.useShortestRotation);
            }
            else if (anim.AnimationType == AnimationType.Position && rect!=null)
            {
                rect.anchoredPosition = anim.positionStart;
                tween = rect.DOAnchorPos(anim.positionEnd, Duration).SetEase(anim.EaseType); 
            }else if(anim.AnimationType == AnimationType.Position)
            {
                this.transform.localPosition = anim.positionStart;
                tween = this.transform.DOLocalMove(anim.positionEnd, Duration).SetEase(anim.EaseType);
            }
            else if (anim.AnimationType == AnimationType.Fade)
            {
                Image img = this.GetComponent<Image>();
                if (img != null)
                {
                    Color col = img.color;
                    col.a = anim.fadeStart;
                    img.color = col;
                    tween = img.DOFade(anim.fadeEnd, Duration).SetEase(anim.EaseType);
                }
                CanvasGroup group = this.GetComponent<CanvasGroup>();
                if(group != null)
                {
                    group.alpha = anim.fadeStart;
                    tween = group.DOFade(anim.fadeEnd, Duration).SetEase(anim.EaseType);
                }
            }
            if (tween != null) // Security
                sequence.Append(tween);
            sequence.AppendInterval(anim.LoopDelay);
            sequence.SetLoops(anim.LoopCount,anim.LoopType);
            sequence.SetDelay(Delay);
        }

        
    }

    IEnumerator WaitAndTrigger;
    IEnumerator iWaitAndTrigger()
    {
        yield return new WaitForSeconds(Delay + Duration);
        OnComplete?.Invoke();
        WaitAndTrigger = null;
    }

    private void OnDisable()
    {
        if (sequence.IsActive())
        {
            sequence.Kill();
        }

        if (WaitAndTrigger != null)
        {
            StopCoroutine(WaitAndTrigger);
        }
    }

    public void ForceDisable()
    {
        OnDisable();
    }
}
