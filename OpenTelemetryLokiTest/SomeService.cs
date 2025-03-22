namespace OpenTelemetryLokiTest
{
    public interface ISomeService
    {
        void LogSomething();
    }
    public class SomeService(ILogger<SomeService> logger) : ISomeService
    {
        public void LogSomething()
        {
            logger.LogWarning("Something logged...");
            logger.LogError("Logged...");
        }
    }
}
