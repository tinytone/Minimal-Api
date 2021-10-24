namespace Api.Framework
{
    public interface IFromQuery
    {
        void BindFromQuery(IQueryCollection queryCollection);
    }
}