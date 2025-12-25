using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(WaitAllLoading());
    }

    IEnumerator WaitAllLoading()
    {
        yield return new WaitForSeconds(2f);
        GlobalSettings.Instance.ChangeScene(Scene.Game, false);
    }
}
