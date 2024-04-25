using System;
using System.Collections.Generic;
using System.Text;

namespace ECCore.Processes
{
    public class ProcessManagerBuilder
    {

        internal ProcessManagerBuilder()
        { }

        private List<ECProcess> processes = new List<ECProcess>();

        public ProcessManagerBuilder WithService(ECProcess process)
        {
            processes.Add(process);
            return this;
        }

        public ProcessManager Build()
        {
            foreach (var process in processes)
            {
                process.Initialise();
            }
            return new ProcessManager(processes.ToArray());
        }

    }
}
