namespace Api.Framework
{
    public interface IHandler
    {
        public Task<object> RunAsync(object request);
    }
}
