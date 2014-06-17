namespace NLog.Performance
{
    interface ILogImplementation 
    {
        string Name { get; }

        void WriteMessage(string message);

        void ClearTargets();

        void AddFileTarget(string fileName);

        void AddBufferredFileTarget(string fileName, int bufferSize);

        void AddAsyncBufferredFileTarget(string fileName, int bufferSize);
    }
}