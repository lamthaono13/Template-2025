using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using Sirenix.OdinInspector;

public class UICloud : MonoBehaviour
{
    [SerializeField] private Transform cloud_l;
    [SerializeField] private Transform cloud_r;
    [SerializeField] private float time = 0.3f;

    private Tween tweenCloudL;
    private Tween tweenCloudR;

    [Button]
    public void OnOpen(Action actionOnOpenDone, string mess)
    {
        //GameManager.Instance.Camera.gameObject.SetActive(true);

        tweenCloudL = cloud_l.DOLocalMove(new Vector3(241, 0, 0), time).SetUpdate(true).OnComplete(() => { actionOnOpenDone?.Invoke(); tweenCloudL = null; });
        tweenCloudR = cloud_r.DOLocalMove(new Vector3(-241, 0, 0), time).SetUpdate(true).OnComplete(() => { tweenCloudR = null; });
        //StartCoroutine(DelayReturn(action));
    }

    //IEnumerator DelayReturn(Action action)
    //{
    //    yield return new WaitForSeconds(1);
    //    cloud_l.DOLocalMove(new Vector3(-1884, 0 ,0), time);
    //    cloud_r.DOLocalMove(new Vector3(1884, 0, 0), time);
    //    action?.Invoke();
    //}


    [Button]
    public void OnClose(Action actionOnOpenDone, string mess)
    {
        //GameManager.Instance.Camera.gameObject.SetActive(false);

        if (tweenCloudL != null)
        {
            tweenCloudL.Complete();

            cloud_l.DOLocalMove(new Vector3(-1884, 0, 0), time).SetUpdate(true).OnComplete(() => { actionOnOpenDone?.Invoke(); tweenCloudL = null; });

            tweenCloudR.Complete();

            cloud_r.DOLocalMove(new Vector3(1884, 0, 0), time).SetUpdate(true);
        }
        else
        {
            cloud_l.DOLocalMove(new Vector3(-1884, 0, 0), time).SetUpdate(true).OnComplete(() => { actionOnOpenDone?.Invoke(); tweenCloudL = null; });
            cloud_r.DOLocalMove(new Vector3(1884, 0, 0), time).SetUpdate(true).OnComplete(() => { tweenCloudR = null; }); ;
        }
    }
}