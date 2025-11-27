using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EventBusTest : MonoBehaviour
{
    [SerializeField] private string testString;

    // Start is called before the first frame update
    void Start()
    {

    }

    Action action;

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button]
    public void SubscribeTestEvent()
    {
        action = AdvancedEventBus.Subscribe<TestEvent>((a) => { Debug.LogError(a.message); });
    }

    [Button]
    public void PublishTestEvent()
    {
        AdvancedEventBus.Publish(new TestEvent { message = testString });
    }

    [Button]
    public void UnsubscribeTestEvent()
    {
        action?.Invoke();
        action = null;
    }
}

public struct TestEvent
{
    public string message;
}