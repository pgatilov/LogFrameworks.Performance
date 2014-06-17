namespace NLog.Performance
{
    class TestCase
    {
        public IScenario Scenario { get; set; }

        public ILogConfiguration Configuration { get; set; }

        public ILogImplementation Logger { get; set; }

        public ILoadProfile LoadProfile { get; set; }
    }
}