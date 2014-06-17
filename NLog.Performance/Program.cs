using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Layouts;
using System.Threading;
using System.Collections.Concurrent;

namespace NLog.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var implementation in CreateLogImplementations())
            {
                foreach (var test in CreateScenarios())
                {
                    foreach (var configuration in CreateLogConfigurations())
                    {
                        foreach (var loadProfile in CreateLoadProfiles())
                        {
                            Console.WriteLine("Press Enter to start test {0} with configuration {1} for implementation {2}, load profile {3}",
                                test.Name,
                                configuration.Name,
                                implementation.Name,
                                loadProfile.Name);
                            Console.ReadLine();

                            var testCase = new TestCase
                            {
                                Configuration = configuration,
                                Logger = implementation,
                                Scenario = test,
                                LoadProfile = loadProfile
                            };

                            RunTest(testCase);
                        }
                    }
                }
            }
        }

        static void RunTest(TestCase testCase) 
        {
            var configuration = testCase.Configuration;
            var test = testCase.Scenario;
            var implementation = testCase.Logger;
            var loadProfile = testCase.LoadProfile;

            Console.WriteLine("Starting test: {0}, config: {1}, implementation: {2}, load profile: {3}", test.Name, configuration.Name, implementation.Name, loadProfile.Name);

            testCase.Configuration.Apply(testCase.Logger);

            // warmup run
            testCase.Scenario.Run(testCase.Logger);

            const int TimesToRun = 10000;
            loadProfile.Run(() => testCase.Scenario.Run(testCase.Logger), TimesToRun);

            Console.WriteLine("Test done: {0}, config: {1}, implementation: {2}, load profile: {3}", test.Name, configuration.Name, implementation.Name, loadProfile.Name);
        }

        static IEnumerable<IScenario> CreateScenarios()
        {
            yield return new LargeMessageScenario();
        }

        static IEnumerable<ILogConfiguration> CreateLogConfigurations()
        {
            yield return new SimpleFileLogConfiguration();
        }

        static IEnumerable<ILogImplementation> CreateLogImplementations() 
        {
            yield return new NLogImplementation();
        }

        static IEnumerable<ILoadProfile> CreateLoadProfiles() 
        {
            yield return new ConstantLoadProfile(25);
        }
    }

    class TestCase
    {
        public IScenario Scenario { get; set; }

        public ILogConfiguration Configuration { get; set; }

        public ILogImplementation Logger { get; set; }

        public ILoadProfile LoadProfile { get; set; }
    }

    interface IScenario
    {
        string Name { get; }

        void Run(ILogImplementation logger);
    }

    class LargeMessageScenario : IScenario 
    {
        private static readonly string Message;

        static LargeMessageScenario()
        {
            Message = new string('a', 100 * 1024);
        }

        public string Name { get { return "Large Message (100 Kb) + 100 ms db query"; } }

        public void Run(ILogImplementation logger)
        {
            logger.WriteMessage(Message);

            Thread.Sleep(TimeSpan.FromMilliseconds(100));
        }
    }

    interface ILogConfiguration
    {
        string Name { get; }

        void Apply(ILogImplementation implementation);
    }

    class SimpleFileLogConfiguration : ILogConfiguration 
    {
        public string Name
        {
            get { return "Simple File Log"; }
        }

        public void Apply(ILogImplementation implementation)
        {
            implementation.ClearTargets();
            implementation.AddFileTarget("log.txt");
        }
    }

    interface ILogImplementation 
    {
        string Name { get; }

        void WriteMessage(string message);

        void ClearTargets();

        void AddFileTarget(string fileName);
    }

    class NLogImplementation : ILogImplementation 
    {
        readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public string Name { get { return "NLog"; } }

        public void ClearTargets() 
        {
            LogManager.Configuration = new LoggingConfiguration();
            LogManager.ReconfigExistingLoggers();
        }

        public void AddFileTarget(string fileName) 
        {
            var configuration = LogManager.Configuration;

            var fileTarget = new FileTarget
            {
                FileName = Layout.FromString(fileName),
                Layout = Layout.FromString("${message}"),
                DeleteOldFileOnStartup = true,
            };
            configuration.AddTarget("file", fileTarget);

            configuration.LoggingRules.Add(new LoggingRule("*", LogLevel.Trace, fileTarget));

            LogManager.ReconfigExistingLoggers();
        }

        public void WriteMessage(string message) 
        {
            _logger.Info(message);
        }
    }

    interface ILoadProfile
    {
        string Name { get; }

        void Run(Action action, int times);
    }

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
            var runsPerThread = times / _parallelUsers;

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

    public class ParallelThreadsTaskScheduler : TaskScheduler
    {
        readonly int _threadCount;
        readonly ConcurrentQueue<Task> _tasks = new ConcurrentQueue<Task>();
        readonly WorkerThread[] _workerThreads;

        public ParallelThreadsTaskScheduler(int threadCount) 
        {
            _threadCount = threadCount;
            _workerThreads = Enumerable.Range(0, threadCount)
                .Select(i => new WorkerThread(this))
                .ToArray();
        }

        public override int MaximumConcurrencyLevel
        {
            get
            {
                return _threadCount;
            }
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Enqueue(task);

            foreach (var thread in _workerThreads) 
            {
                var wokeThread = thread.TryWake();
                if (wokeThread) 
                {
                    break;
                }
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return _tasks.ToArray();
        }

        protected override bool TryDequeue(Task task)
        {
            return false;
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        class WorkerThread 
        {
            readonly Thread _thread;
            readonly ParallelThreadsTaskScheduler _scheduler;

            AutoResetEvent _workerLoopSleepAlarm;
            volatile bool _isSleeping;

            public WorkerThread(ParallelThreadsTaskScheduler scheduler) 
            {
                _scheduler = scheduler;
                _thread = new Thread(WorkerLoop) 
                {
                    IsBackground = true
                };
            }

            private void WorkerLoop() 
            {
                using (_workerLoopSleepAlarm = new AutoResetEvent(false))
                {
                    while (true)
                    {
                        Task task;
                        var haveWork = _scheduler._tasks.TryDequeue(out task);
                        if (!haveWork)
                        {
                            _isSleeping = true;
                            _workerLoopSleepAlarm.WaitOne();
                            _isSleeping = false;

                            continue;
                        }

                        _scheduler.TryExecuteTask(task);
                    }
                }
            }

            public bool TryWake() 
            {
                if (_thread.ThreadState.HasFlag(ThreadState.Unstarted)) 
                {
                    _thread.Start();
                    return true;
                }

                if (!_isSleeping) 
                {
                    return false;
                }

                _workerLoopSleepAlarm.Set();
                return true;
            }
        }
    }
}
