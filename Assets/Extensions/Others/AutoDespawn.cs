using System.Collections;
using System.Collections.Generic;
using ThangDD;
using UnityEngine;

public class AutoDespawn : MonoBehaviour
{
    public float delayTime = 1f;

    void OnEnable()
    {
        Utils.Delay(delayTime, () => { ObjectPool.DeSpawn(this); });
    }
}
