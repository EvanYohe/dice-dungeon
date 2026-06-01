namespace DiceDungeon.scripts.Event;

// Represents a wrapper class for Events running through the Eventbus. It could potentially be
// extended to include additional information like an identifier for the Event, an identifier
// for the Event source, etc... if needed during future development.

public abstract record Event {
    // public string id { get; set; } // Identifier for the Event.
    // public Guid source { get; set; } // Identifier for the source entity of the Event.
}