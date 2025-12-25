using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class Toast : MonoBehaviour
{
    public TMP_Text tvMessage;
    public RectTransform rectBG;
    public RectTransform rectTxt;
    public CanvasGroup canvasGroup;

    public bool isShowed = false;

    public void SetMessage(string mess)
    {
        StopAllCoroutines();
        StartCoroutine(Show(mess));
    }

    IEnumerator Show(string mess)
    {
        isShowed = true;
        tvMessage.text = mess;
        rectBG.transform.localScale = new Vector3(1, 0, 1);
        canvasGroup.alpha = 0;

        rectBG.DOScaleY(1, 0.2f);
        canvasGroup.DOFade(1, 0.1f);

        yield return new WaitForSeconds(1.8f);

        rectBG.DOScaleY(0, 0.2f);
        canvasGroup.DOFade(0, 0.1f);

        yield return new WaitForSeconds(0.2f);
        isShowed = false;
    }
}
