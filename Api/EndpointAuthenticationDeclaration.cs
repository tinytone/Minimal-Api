namespace Api
{
    public class EndpointAuthenticationDeclaration
    {
        public static void Anonymous(params IEndpointConventionBuilder[] builder)
        {
            foreach (var endpoint in builder)
            {
                endpoint.AllowAnonymous();
            }
        }

        public static void Admin(params IEndpointConventionBuilder[] builder)
        {
            foreach (var endpoint in builder)
            {
                endpoint.RequireAuthorization("admin");
            }
        }
    }
}
