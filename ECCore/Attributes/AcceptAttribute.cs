using System;
using System.Collections.Generic;
using System.Text;

namespace ECCore.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AcceptAttribute : Attribute
    {

        public SignalFlags SignalFlags;

        public AcceptAttribute(SignalFlags signalFlags)
        {
            SignalFlags = signalFlags;
        }
    }

    public enum SignalFlags : byte
    {
        SERVER_ONLY = 0,
        FROM_CLIENT = 1,
    }
}
