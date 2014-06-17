namespace NLog.Performance
{
    interface ILogConfiguration
    {
        string Name { get; }

        void Apply(ILogImplementation implementation);
    }
}