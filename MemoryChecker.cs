using System.Diagnostics;

namespace AspNetCoreFirstApp
{
    public class MemoryChecker : IMemoryCheck
    {
        private readonly Process currentProcess;
        public MemoryChecker()
        {
            currentProcess = Process.GetCurrentProcess();
        }
        public string MemoryUsedByApplication()
        {
            currentProcess.Refresh();
            var usedMemory = $"Memory used by this application: {currentProcess.WorkingSet64 / 1024}";
            return usedMemory;
        }
    }
}
