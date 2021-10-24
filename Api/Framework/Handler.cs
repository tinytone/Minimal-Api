namespace Api.Framework
{
    /// <summary>
    /// Mimic the classes that Mediatr would use
    /// </summary>
    public abstract class Handler<TRequest, TOut> : IHandler where TRequest : IRequest<TOut>
    {
        public abstract Task<TOut> Run(TRequest request);

        public async Task<object> RunAsync(object request)
        {
            var result = await Run((TRequest)request);
            return result;
        }
    }
}
