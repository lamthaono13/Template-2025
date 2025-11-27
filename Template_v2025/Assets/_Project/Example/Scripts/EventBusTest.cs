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

    public void SubscribeTestEvent()
    {
        action = AdvancedEventBus.Subscribe<TestEvent>((a) => { });
    }

    public void PublishTestEvent()
    {
        AdvancedEventBus.Publish(new TestEvent { message = testString });
    }

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