using DiceDungeon.scripts.Event;
using Godot;

namespace DiceDungeon.scenes.Entities.PlayerEntity;

public partial class PlayerEntity : CharacterBody2D {
	
	[Export] public float speed { get; set; }
	[Export] public float health { get; set; }
	
	private BadTestArea _badTestArea;

	public override void _Ready() {
		
		this._badTestArea.EntitySpaceEntered += OnEntitySpaceEntered;
		EventBus.subscribe<EntitySpaceEnteredEvent>(onEntitySpaceEnteredEvent);
	}
	
	public override void _ExitTree() {
	}

	public override void _Process(double delta) {
		Vector2 playerDirectionVector = Input.GetVector(
			"move_left",
			"move_right",
			"move_up",
			"move_down");
		
		this.Velocity = playerDirectionVector * (this.speed / 10);
		MoveAndCollide(this.Velocity);
	}
	
	
	// Delegate function for the EntitySpaceEnteredEvent using my EventBus.
	// Takes the Event itself as parameter(s).
	private void onEntitySpaceEnteredEvent(EntitySpaceEnteredEvent e) {
		GD.Print($"Entity entered space by: {e.body}");
	}
	
	// Delegate function for the EntitySpaceEnteredEvent using the Godot signal system.
	// Takes whatever information the emitting entity requires.
	private void OnEntitySpaceEntered(Node2D body) {
		GD.Print($"Entity entered space by: {body.Name}");
	}
}
