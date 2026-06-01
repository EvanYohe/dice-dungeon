using DiceDungeon.scripts.Event;
using Godot;

namespace DiceDungeon.scenes.Entities.PlayerEntity;

public partial class PlayerEntity : CharacterBody2D {
	
	[Export] 
	public float Speed { get; set; }
	[Export] 
	public float Health { get; set; }
	[Export] 
	private BadTestArea _badTestArea;

	public override void _Ready() {
		
		_badTestArea.EntitySpaceEntered += OnEntitySpaceEntered;
		// My implementation of an Event subscription using the EventBus.
		// EventBus.subscribe<EntitySpaceEnteredEvent>(onEntitySpaceEnteredEvent);
	}

	public override void _ExitTree() {
		
		_badTestArea.EntitySpaceEntered -= OnEntitySpaceEntered;
		// My implementation of an Event unsubscription using the EventBus.
		// EventBus.unsubscribe<EntitySpaceEnteredEvent>(onEntitySpaceEnteredEvent);
	}

	public override void _Process(double delta) {
		
		Vector2 playerDirectionVector = Input.GetVector(
			"move_left",
			"move_right",
			"move_up",
			"move_down");

		Velocity = playerDirectionVector * (Speed / 10);
		MoveAndCollide(Velocity);
	}


	// Delegate function for the EntitySpaceEnteredEvent using my EventBus.
	// Takes the Event itself as parameter(s).
	private void OnEntitySpaceEnteredEvent(EntitySpaceEnteredEvent e) {
		
		GD.Print($"Entity entered space by: {e.Body}");
	}

	// Delegate function for the EntitySpaceEnteredEvent using the Godot signal system.
	// Takes whatever information the emitting entity requires.
	private void OnEntitySpaceEntered(Node2D body) {
		
		GD.Print($"Entity entered space by: {body.Name}");
	}
}
