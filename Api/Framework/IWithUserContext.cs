using System.Security.Claims;

namespace Api.Framework
{
    public interface IWithUserContext
    {
        void BindFromUser(ClaimsPrincipal user);
    }
}
