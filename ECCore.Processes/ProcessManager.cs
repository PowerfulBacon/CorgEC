using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ECCore.Processes
{
    public class ProcessManager
    {

        /// <summary>
        /// The amount of time that we are allowed to go over the allocated tick rate for.
        /// Ensures that we can still do some processing, even if the blackbox part of the
        /// application that we can't see consumes all of our tick.
        /// This will however, cause the global tick rate to decrease, which will lower the
        /// tick rate of our application overall.
        /// Worst case scenario:
        /// - Application spends the entire tickRate time processing.
        /// - We are forced to use all of the overtime amount.
        /// This means that a total of TickRate + OvertimeAmount will be the slowest that
        /// we can bring the application to ourselves, however if the application is itself
        /// overtiming, then that will lower the tick rate even more.
        /// </summary>
        public TimeSpan OvertimeAmount { get; set; } = TimeSpan.FromMilliseconds(10);

        /// <summary>
        /// The tickrate that we want the application to have.
        /// </summary>
        public TimeSpan TickRate { get; set; } = TimeSpan.FromMilliseconds(50);

        /// <summary>
        /// The last time that we fired at. Determines how much we can run the tick for.
        /// </summary>
        private DateTime? lastFire = null;

        /// <summary>
        /// The systems that have been queued to fire in order of the time that they should fire at.
        /// </summary>
        private SortedList<DateTime, ECProcess> QueuedSystems = new SortedList<DateTime, ECProcess>();

        /// <summary>
        /// Array of the currently executing systems by ID, or null
        /// </summary>
        private ECProcess[] RunningSystems;

        internal ProcessManager(ECProcess[] systems)
        {
            this.RunningSystems = systems;
        }

        public static ProcessManagerBuilder FromBuilder()
        {
            return new ProcessManagerBuilder();
        }

        /// <summary>
        /// Fire the process manager and trigger any updates that need to be
        /// performed.
        /// Handles lag mitigation by scheduling things to keep the tick length
        /// below what we requested to prevent overtime.
        /// If we have too much to process, then we need to choose how we slow down
        /// all that processing and reduce the tick speed.
        /// </summary>
        public void Fire()
        {
            // Time spent running application blackbox
            TimeSpan timeSinceLastTick = lastFire != null ? DateTime.Now - lastFire.Value : TimeSpan.Zero;
            Fire(timeSinceLastTick);
        }

        /// <summary>
        /// Fire the process manager and trigger any updates that need to be
        /// performed.
        /// Handles lag mitigation by scheduling things to keep the tick length
        /// below what we requested to prevent overtime.
        /// If we have too much to process, then we need to choose how we slow down
        /// all that processing and reduce the tick speed.
        /// </summary>
        /// <param name="elapsedTime">Manually override the elapsed time, to perform testing or provide custom time calculations.</param>
        public void Fire(TimeSpan elapsedTime)
        {
            // Determine how long we can run for
            // If it took 20ms since our last tick, we can run for 30ms
            TimeSpan allowedExecutionTime = TickRate - elapsedTime;
            // Determine if we need to enter overtime
            if (allowedExecutionTime < OvertimeAmount)
            {
                allowedExecutionTime = OvertimeAmount;
            }
        }

    }
}
