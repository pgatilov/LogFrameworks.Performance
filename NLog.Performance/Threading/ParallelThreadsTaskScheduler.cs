using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NLog.Performance.Threading
{
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