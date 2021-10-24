using Api.Data;
using Api.Framework;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class TestRequest : IRequest<object>, IFromJsonBody, IFromRoute, IFromQuery
    {
        public string Title { get; set; }

        public int FromRoute { get; set; }

        public string FromQuery { get; set; }

        public void BindFromQuery(IQueryCollection queryCollection)
        {
            if (queryCollection.TryGetValue("v", out var v))
            {
                this.FromQuery = v;
            }
        }

        public void BindFromRoute(RouteValueDictionary routeValues)
        {
            this.FromRoute = routeValues.GetInt("id");
        }
    }

    public class Test: Handler<TestRequest, object>
    {
        public override Task<object> Run(TestRequest request)
        {
            return Task.FromResult((object)new
            {
                result = $"{request.Title}_{request.FromRoute}_{request.FromQuery}"
            });
        }
    }
}

 