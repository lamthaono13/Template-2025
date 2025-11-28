// IEventBus.cs
using System;

public interface IEventBus
{
    // Subscribe returns an Action token that unsubscribes when invoked.
    Action Subscribe<T>(Action<T> handler, int priority = 0, bool once = false, bool receiveSticky = false);

    // Publish an event payload of type T. If sticky==true then payload stored for future receiveSticky subscribers.
    void Publish<T>(T payload, bool sticky = false);

    // Unsubscribe by handler (optional safe removal).
    void Unsubscribe<T>(Action<T> handler);

    // Clear sticky event for type T.
    void ClearSticky<T>();

    // Clear all sticky events.
    void ClearAllSticky();

    // Check if there are subscribers for type T.
    bool HasSubscribers<T>();

    // Debug helper: returns number of subscribers for type T.
    int SubscriberCount<T>();

    // Completely clear subscribers and sticky events (use with care).
    void ClearAll();
}