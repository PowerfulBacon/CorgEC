using System;
using System.Collections.Generic;
using System.Text;

namespace ECCore.Processes
{
    public enum ECProcessFlags
    {
        /// <summary>
        /// No flags
        /// </summary>
        NONE = 0,
        /// <summary>
        /// This process runs in real time rather than tick time, meaning that if
        /// it took 52ms since the last time it fired, that number will be passed instead
        /// of the tick delay.
        /// With this disabled, processes will have a constant delta time equivilent to
        /// their requested delay, even if they aren't actually processing at that speed
        /// due to lag mitigation.
        /// </summary>
        REALTIME_PROCESSING = (1 << 0),
        /// <summary>
        /// This process never actually fires and is only used for initialisation and
        /// data storage.
        /// </summary>
        NO_FIRE = (1 << 1),
    }
}
