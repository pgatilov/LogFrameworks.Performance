namespace NLog.Performance
{
    interface ILogImplementation 
    {
        string Name { get; }

        void WriteMessage(string message);

        void ClearTargets();

        void AddFileTarget(string fileName, bool keepOpen, bool allowLocalWrite);

        void AddBufferredFileTarget(string fileName, int bufferSize, bool exclusive);

        void AddAsyncBufferredFileTarget(string fileName, int bufferSize, bool exclusive);
    }
}