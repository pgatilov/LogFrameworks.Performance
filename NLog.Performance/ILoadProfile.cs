using System;

namespace NLog.Performance
{
    interface ILoadProfile
    {
        string Name { get; }

        void Run(Action action, int times);
    }
}