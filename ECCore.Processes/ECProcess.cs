using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ECCore.Processes
{
    public abstract class ECProcess
    {

        /// <summary>
        /// The flags that govern how this process operates
        /// </summary>
        protected internal abstract ECProcessFlags Flags { get; }

        /// <summary>
        /// Time between individual fires of this process
        /// </summary>
        protected internal abstract TimeSpan Delay { get; }

        /// <summary>
        /// Initialise the process, run once at the start of the application
        /// </summary>
        protected internal abstract void Initialise();

        /// <summary>
        /// Indicate that something will perform processing when we fire.
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="processor"></param>
        /// <param name="poolId">
        /// The pool ID for the processor. Things with the same poolID will share the same tick, if there are multiple
        /// pool IDs, then the process' allocated tick will be distributed among them fairly, with any leftover tick
        /// being given to anyone who overtimed.
        /// </param>
        protected void AddProcessor<TType>(ECProcessor<TType> processor)
        {
            attachedProcesses.Add(processor);
        }

        List<IECProcessor> attachedProcesses = new List<IECProcessor>();

        List<IEnumerator> runningProcesses = new List<IEnumerator>();

        /// <summary>
        /// Create the process.
        /// </summary>
        /// <param name="timeElapsed"></param>
        /// <param name="allocatedTime">The amount of time that we were allocated to fire</param>
        /// <returns></returns>
        internal void _Process(TimeSpan timeElapsed, TimeSpan allocatedTime)
        {
            // Process the system and collect all the things that we need to fire
            if (runningProcesses.Count == 0)
            {
                Process(timeElapsed);
                foreach (var process in attachedProcesses)
                {
                    runningProcesses.Add(process.PerformRun(timeElapsed).GetEnumerator());
                }
            }
            // Continue running until we run out of tick, or run out of things to process
            while (allocatedTime.TotalMilliseconds > 0 && runningProcesses.Count > 0)
            {
                TimeSpan dividedTick = TimeSpan.FromTicks(allocatedTime.Ticks / runningProcesses.Count);
                Stopwatch timeMonitor = new Stopwatch();
                timeMonitor.Start();
                for (int i = runningProcesses.Count - 1; i >= 0; i--)
                {
                    // Get the thing that we need to process
                    IEnumerator processEnumerator = runningProcesses[i];
                    // Keep processing until we run out of allowed time
                    while (timeMonitor.Elapsed < dividedTick)
                    {
                        // Completed processing
                        if (!processEnumerator.MoveNext())
                        {
                            runningProcesses.Remove(processEnumerator);
                            break;
                        }
                    }
                    // Remove the time that we spent from the time allocated
                    allocatedTime -= timeMonitor.Elapsed;
                    timeMonitor.Restart();
                }
            }
        }

        /// <summary>
        /// Process the whole system and perform any shared execution functions
        /// </summary>
        /// <param name="timeElapsed"></param>
        protected virtual void Process(TimeSpan timeElapsed)
        { }

    }

    internal interface IECProcessor
    {
        IEnumerable PerformRun(TimeSpan timeElapsed);
    }

    public abstract class ECProcessor<TProcessType> : IECProcessor
    {

        IEnumerable IECProcessor.PerformRun(TimeSpan timeElapsed)
        {
            var collectedItems = Collect(timeElapsed);
            foreach (var item in collectedItems)
            {
                Process(item, timeElapsed);
                yield return null;
            }
        }

        /// <summary>
        /// Collect all the items that we wish to process
        /// </summary>
        /// <param name="timeElapsed"></param>
        /// <returns></returns>
        protected internal abstract IEnumerable<TProcessType> Collect(TimeSpan timeElapsed);

        /// <summary>
        /// Process a single item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="timeElapsed"></param>
        protected internal abstract void Process(TProcessType item, TimeSpan timeElapsed);

    }

}
