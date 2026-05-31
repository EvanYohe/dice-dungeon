using System;
using System.Collections.Generic;
using Godot;

namespace DiceDungeon.scripts.Event;

// Example usage:
// EventBus.subscribe<PlayerSprintEvent>(onPlayerSprintEvent);
// A specific entity  will subscribe to an Event and that specific entity will contain the logic
// for processing the Event when it's triggered.

// EventBus.unsubscribe<PlayerSprintEvent>(onPlayerSprintEvent);
// Pretty self-explanatory, this will remove the callback function of the listening entity from
// the list of subscribers for that Event type.

// EventBus.publish(new PlayerSprintEvent());
// An entity will use EventBus.publish() to trigger the callback functions associated with all
// the entities that are subscribed to the Event type.

public partial class EventBus : Node {
	
    // Dictionary that stores Events and their corresponding subscribers where the subscribers
    // are represented by the functions that will be called when the Event is triggered.
    private static readonly Dictionary<Type, List<Delegate>> SUBSCRIBERS = new Dictionary<Type, List<Delegate>>();

    // Called by something that wants to know when an Event happens, where the callback is the
    // function that the subscriber will process on the Event being triggered (published).
    public static void subscribe<T>(Action<T> callback) where T : Event {
        
        Type eventType = typeof(T);
        
        // Check if the Event type is already recorded in the SUBSCRIBERS dictionary and creates
        // a new List of subscribers (callbacks) for that type of Event if it doesn't exist.
        if (!SUBSCRIBERS.TryGetValue(eventType, out List<Delegate> callbacks)) {
            callbacks = new List<Delegate>();
            SUBSCRIBERS[eventType] = callbacks;
        }
        
        // Check if the callback function is already in the list of subscribers for this type
        // of Event and add it to the list if not already an element of the list.
        if (!callbacks.Contains(callback)) {
            callbacks.Add(callback);
        }
    }

    // Called by something that no longer wants to know when an Event happens,
    // where the callback is a function in List<Delegate> corresponding to the Event type.
    public static void unsubscribe<T>(Action<T> callback) where T : Event {
        
        Type eventType = typeof(T);
        
        // Check if the Event type has any subscribers in the SUBSCRIBERS dictionary.
        if (!SUBSCRIBERS.TryGetValue(eventType, out var callbacks)) {
            return;
        }
        
        // Remove the callback function from the list of subscribers for this type of Event.
        callbacks.Remove(callback);
        
        // If no subscribers exist for this Event type, remove the entry from the dictionary.
        if (callbacks.Count == 0) {
            SUBSCRIBERS.Remove(eventType);
        }
    }

    // Called by something that wants to trigger the callback functions associated with the Event
    // type passed as the parameter.
    public static void publish<T>(T evt) where T : Event {
        
        Type eventType = typeof(T);
        
        // Check if the Event type has any subscribers in the SUBSCRIBERS dictionary.
        if (!SUBSCRIBERS.TryGetValue(eventType, out var callbacks)) {
            return;
        }
        
        // Iterates over a copy of the subscribers to the Event type and trigger the callback
        // function associated with each subscriber.
        foreach (Delegate callback in callbacks.ToArray()) {
            ((Action<T>)callback).Invoke(evt);
        }
    }
}