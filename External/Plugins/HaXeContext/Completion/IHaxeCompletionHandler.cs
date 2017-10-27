namespace HaXeContext
{
    public interface ICompletionServerCompletionHandler : IHaxeCompletionHandler
    {
        event FallbackNeededHandler FallbackNeeded;
    }

    public interface IHaxeCompletionHandler
    {
        string GetCompletion(string[] args);
        string GetCompletion(string[] args, string fileContent);
        void Stop();
    }
}
