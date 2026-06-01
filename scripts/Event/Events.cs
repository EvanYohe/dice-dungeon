using Godot;

namespace DiceDungeon.scripts.Event;

// System Level Events

// UI Events

// Combat Events

// Map Generation Events

// Entity State Events
public sealed record EntitySpaceEnteredEvent(Node2D Body) : Event;