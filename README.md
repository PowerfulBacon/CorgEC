# CorgEC

CorgEC is a component for implementing the Entity/Component model. CorgEC offers a highly transparent
networking model meaning that you can program networked applications without having to worry about the
lower level networking.

CorgEC is primarilly intended for games, but can be used for any application that requires a networked,
or offline, component model.

# Definitions

**Entity**: An Entity is an object that holds components and can have signals raised against it.
Entities can be accessed by identifier.

**Component**: A component holds data. It is attached to an Entity and can respond to signals raised
against its Entity with specialised logic.

**Signal**: A signal is something that happens to an Entity which can be responded to, but may not
necessarilly be responded to. These are used to communicate things happening in a world.

**Instance**: An instance represents a container for holding all of the Entities that exist.

# Quick-Start

## E/C

```cs
// Execute main code
// Create an entity and add the balance reporter component to it
Entity reporterEntity = Instance.DefaultInstance.Create(entity => {
    entity.TryAddComponent(new BalanceReporterComponent());
});
// Get the signal context so that we can raise signals against the entity.
// You can cache this for better performance when signals are raised frequently.
var signalContext = reporterEntity.GetSignalContext<BalanceUpdatedSignal>();
// Raise a signal against that entity, simulating a balance change
signalContext.Raise(new BalanceUpdatedSignal(10, 30));


// Create an example component which responds to a signal
class BalanceReporterComponent : Component<BalanceReporterComponent>
{
    
    // OnSignal indicates that this will be called when a signal is raised against
    // our parent.
    // Parameters can be supplied which specify network security flags, by default
    // signals will be ignored if they don't come from yourself.
    [OnSignal]
    public void OnBalanceUpdated(BalanceUpdatedSignal signal)
    {
        Console.WriteLine($"Balance changed from {signal.OldBalance} to {signal.NewBalance}!");
    }

}

// Create a signal which represents something happening
// This signal represents the balance of an account being updated.
// Signals are used to respond to behaviour, and should be used when the handling
// of the signal is optional. They should't be used when you want to check if the balance
// can be updated, use GetComponent<> and a function call instead.
class BalanceUpdatedSignal : Signal<BalanceUpdatedSignal>
{
    public int OldBalance { get; }
    public int NewBalance { get; }
    public BalanceUpdatedSignal(int old, int @new)
    {
        OldBalance = old;
        NewBalance = @new;
    }
}
```

## Networking

The networking library is built on-top of a high-level networking library. A TCP server/client
implementation has been provided, however you can create custom implementations of the `INetworkHost`
and `INetworkInterface` to use a different transport layer (this allows future room for expansion
into using different systems such as WebSockets, UDP Clients, or SteamRelay).

To setup basic networking using the default TCP implementation, you can use the following code:

```cs
```

### Ownership

When working with CorgEC over networks, the concept of ownership is introduced. There are 4 different
types of clients and the definition depends on the Entity that you are currently working on:
- Owner: The person that holds ownership over the Entity.
- Server: The server instance (which is also the owner of all Entities by default).
- Client: All client instances connected to the server.
- Self: The current local instance.

Ownership by itself does not provide any additional behaviour, however it can be used to represent
that a client is allowed to perform certain actions on a specific Entity. For example, if each connecting
client needs to have a player entity, then the player entity will have ownership given to the client
that the player belongs to. This could allow for the client to move their player around, without
allowing other clients to move someone else's player around.

You can indicate who an event is allowed to be called by, and who an event should be called on
by adding parameters to the `OnSignal` attributes.

### Networked Signals

The networking of signals is handled automatically and is designed to be as transparent as possible
in order to minimise the overhead of coding networked applications.

It works by using the following ideas:
- If the server knows that a signal needs to be raised on clients, then it will network the signal
and send it over the network.
- If the client knows that a signal needs to be raised on the server, then it will network the signal
and send it over the network.
- If the server has a signal which is only raised on the server, then it will not network the signal.
- If a client has a siganl which is only raised on itself, then it will not network the signal.
- If a client has a signal which needs to be raised on other clients, then it will send the signal to
the server, where it is relayed to other clients.

This process works entirely based off of the `OnSignal` attributes and is entirely transparent. The user
never has to specify that a signal needs to be sent over the network, and never has to introduce
spaghetti code while trying to transmit different signals around in inconsistent ways.

### Network Prediction

Communicating over networks introduces latency. How to handle latency depends on implementation/application
specifics and it cannot be handled in a generic way. CorgEC allows for users to implement their own
network prediction functions by passing over the delta time between when a message was sent vs
when it was recieved. This is accessed by adding a double, a TimeSpan, or a float parameter to any function marked with
the `OnSignal` attribute.

```cs
void OnMovedA(MoveSignal signal, TimeSpan latency)
{
    model.Position = signal.newPosition + signal.velocity * latency.TotalSeconds;
}

void OnMovedB(MoveSignal signal, double latency)
{
    model.Position = signal.newPosition + signal.velocity * latency;
}

void OnMovedC(MoveSignal signal, float latency)
{
    model.Position = signal.newPosition + signal.velocity * latency;
}
```

All of the above functions will give the same results, different types for latency are provided
for convenience.
