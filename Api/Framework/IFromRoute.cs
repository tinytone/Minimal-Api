namespace Api.Framework
{
    public interface IFromRoute
    {
        void BindFromRoute(RouteValueDictionary routeValues);
    }
}