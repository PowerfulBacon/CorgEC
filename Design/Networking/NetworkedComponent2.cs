
using System.ComponentModel;

public class NetworkedComponentExample : Component
{

    [OnSignal(AcceptFrom.Anyone, RunOn.Server)]
    public void HandleUserInput(GenericSignal signal)
    {

    }

    [OnSignal(AcceptFrom.Server, RunOn.Everyone)]
    public void HandleServerCommand(GenericSignal signal)
    {

    }

    [OnSignal(AcceptFrom.Owner, RunOn.Server)]
    public void HandleOwnerCommand(GenericSignal signal)
    {

    }

}

public class GenericSignal { }
