
/// <summary>
/// This type of component only initialises on the client.
/// TODO: Refactor client component to owner component, there is no need for a concept of clients, only for the concept of
/// object owners. Clients are a low-level system and shoulds be entirely transparent.
/// </summary>
[RequiresOwnership]
public class ClientExample : Component
{

    public override void Initialise()
    {
        // This runs on both the server and the client, the server registers the signal
        // and sets up a handler that relays any server signals to the client to be processed.
        // QUESTION: Who is going to be the signal handler?
        //      ANSWER: The owner of the entity, which can be the server or another client.
        // QUESTION: How can we transparently handle both server and client signals?
        //      ANSWER: When a server signal is triggered on this entity server-side, then it
        //          will be communicated to the owner client (if it hasn't been communicated already).
        //          See server component for how it works the other way around.
        Register<ClientSignal>(signal => {
            // Signal handler here
        });
        Register<ServerSignal>(signal => {
            // Signal handler here
        });
    }

}

public class ServerExample : Component
{

    public override void Initialise()
    {
        // This only runs on the server
        // QUESTION: How will the server know about client signals?
        //      ANSWER: All client signals will be sent to the server when they are raised, regardless of if they are handled or not.
        //      ANSWER 2: Client signals should never be handled by server examples, since client signals can only be sent to objects that a client holds ownership over.
        //          Only client components can have the notion of ownership.
        //      ANSWER 3: Ownership is a concept on entities, not components. This means that if a client signal is raised on an entity that the client owns,
        //          then the client could send that signal to the server to be handled by a server component if needed.
        //          Note that the server knows everything, but clients do not. This means that clients always send their signals to the server, but the server
        //          only sends their signals if necessary.
        //      ADDENDUM: Client signals can only be raised against objects that the client owns, which means they fully understand that object.
        //      ADDENDUM 2: Clients may not know about server components that are stored on their owned objects (in some cases?).
        Register<ClientSignal>(signal => {
            // Signal handler here
        });
        // Easy case, just handle as normal.
        Register<ServerSignal>(signal => {
            // Signal handler here
        });
    }

}

// Any signal raised on a client
// Will only be accepted by the server if it has client raise flags allowed
[Accept(SignalFlags.FROM_CLIENT)]
public class ClientSignal { }

// Any signal raised on the server
public class ServerSignal { }
