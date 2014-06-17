using System;
using System.Collections.Generic;
using NLog.Performance.LoadProfiles;
using NLog.Performance.LogImplementations;
using NLog.Performance.PerformanceMonitor;

namespace NLog.Performance
{
    class Program
    {
        static void Main()
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

            Console.ReadLine();
        }

        static void RunTest(TestCase testCase)
        {
            using (var performanceMonitor = CreatePerformanceMonitor())
            {
                Console.WriteLine("Starting test...");

                testCase.Configuration.Apply(testCase.Logger);

                // warmup run
                testCase.Scenario.Run(testCase.Logger);

                performanceMonitor.Start();
                const int TimesToRun = 10000;
                testCase.LoadProfile.Run(() => testCase.Scenario.Run(testCase.Logger), TimesToRun);
                performanceMonitor.Stop();

                Console.WriteLine("Test done");

                Console.WriteLine("Performance report:");
                Console.WriteLine(performanceMonitor.GetPerformanceStatistics());
            }
        }

        private static IPerformanceMonitor CreatePerformanceMonitor()
        {
            return new PerformanceMonitor.PerformanceMonitor();
        }

        static IEnumerable<IScenario> CreateScenarios()
        {
            yield return new LargeMessageScenario();
        }

        static IEnumerable<ILogConfiguration> CreateLogConfigurations()
        {
            yield return new SimpleFileLogConfiguration();
            yield return new ExclusiveFileLogConfiguration();
            yield return new BufferredFileLogConfiguration();
            yield return new AsyncBufferredFileLogConfiguration();
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
}
