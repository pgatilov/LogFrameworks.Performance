namespace NLog.Performance
{
    interface IScenario
    {
        string Name { get; }

        void Run(ILogImplementation logger);
    }
}