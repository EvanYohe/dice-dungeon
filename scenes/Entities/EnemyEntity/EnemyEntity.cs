using Godot;
using Godot.Collections;

namespace DiceDungeon.scenes.Entities.EnemyEntity;

public partial class EnemyEntity : CharacterBody2D {
	[Export] public Dictionary<string, int> bonusDiceDamage = new() {
		{ "d4", 0 },
		{ "d6", 0 },
		{ "d8", 0 },
		{ "d10", 0 },
		{ "d12", 0 },
		{ "d20", 0 }
	};

	[Export] public Dictionary<string, int> bonusTypeDamage = new() {
		{ "Physical", 0 },
		{ "Fire", 0 },
		{ "Cold", 0 },
		{ "Lightning", 0 },
		{ "Thunder", 0 },
		{ "Poison", 0 },
		{ "Acid", 0 },
		{ "Radiant", 0 },
		{ "Necrotic", 0 },
		{ "Force", 0 }
	};

	[Export] public float Speed { get; set; }
	[Export] public int Health { get; set; }
	[Export] public int Mana { get; set; }
	[Export] public int Shield { get; set; }
	[Export] public int Initiative { get; set; }
	[Export] public int HandSize { get; set; }
	[Export] public int Rerolls { get; set; }


	public override void _Ready() {
	}

	public override void _Process(double delta) {
	}
}
