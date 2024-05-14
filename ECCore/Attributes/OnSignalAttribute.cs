using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECCore.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    [MeansImplicitUse(ImplicitUseTargetFlags.WithMembers | ImplicitUseTargetFlags.WithInheritors)]
    public class OnSignalAttribute : Attribute
    {

        public AcceptFrom AcceptFrom;

        public RunOn RunOn;

        public Type acceptedSignalType;

        public Action<object> registrationAction;

        public OnSignalAttribute(AcceptFrom acceptFrom, RunOn runOn)
        {
            AcceptFrom = acceptFrom;
            RunOn = runOn;
        }

        public OnSignalAttribute()
        {
            AcceptFrom = AcceptFrom.Self;
            RunOn = RunOn.Self;
        }
    }

    public enum AcceptFrom : byte
    {
        Server = (1 << 0),
        Client = (1 << 1),
        Owner = (1 << 2),
        Anyone = (1 << 3),
        Self = (1 << 4),
    }

    public enum RunOn : byte
    {
        Server = (1 << 0),
        Client = (1 << 1),
        Owner = (1 << 2),
        Everyone = (1 << 3),
        Self = (1 << 4),
    }

    public enum SentFrom : byte
    {
        Server = (1 << 0),
        Client = (1 << 1),
        Owner = (1 << 2),
        Self = (1 << 4),
    }

}
