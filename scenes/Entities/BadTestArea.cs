using Godot;

namespace DiceDungeon.scenes.Entities;

public partial class BadTestArea : Area2D {
    // 
    [Signal]
    public delegate void EntitySpaceEnteredEventHandler(Node2D body);

    public override void _Ready() {
        BodyEntered += OnBodyEntered;
    }

    public override void _ExitTree() {
        BodyEntered -= OnBodyEntered;
    }

    public override void _Process(double delta) {
    }

    // OnBodyEntered is a signal handler for the BodyEntered signal.
    // Parameters can be added to the signal based on whatever information is needed.
    private void OnBodyEntered(Node2D body) {
        // Example of my implementation of an Event managed by the EventBus
        // converted to a signal. My implementation publishes the event to the EventBus
        // where the EventBus contains a list of subscribers and the delegate functions.

        // The Godot signal system directly connects the emitting entity to the
        // receiving entity.

        // EventBus.publish(new EntitySpaceEnteredEvent(body));
        EmitSignal(SignalName.EntitySpaceEntered, body);
    }
}