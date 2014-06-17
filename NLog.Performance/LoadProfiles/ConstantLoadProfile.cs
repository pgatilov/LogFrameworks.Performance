using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NLog.Performance.Threading;

namespace NLog.Performance.LoadProfiles
{
    class ConstantLoadProfile : ILoadProfile 
    {
        private readonly int _parallelUsers;

        public ConstantLoadProfile(int parallelUsers) 
        {
            _parallelUsers = parallelUsers;
        }

        public string Name { get { return string.Format("Constant Load, {0} users.", _parallelUsers); } }

        public void Run(Action action, int times)
        {
            var scheduler = new ParallelThreadsTaskScheduler(_parallelUsers);
            var tasks = Enumerable.Range(0, times)
                                  .Select(i => Task.Factory.StartNew(
                                      action,
                                      CancellationToken.None,
                                      TaskCreationOptions.LongRunning | TaskCreationOptions.PreferFairness | TaskCreationOptions.HideScheduler,
                                      scheduler))
                                  .ToArray();
            Task.WaitAll(tasks);
        }
    }
}