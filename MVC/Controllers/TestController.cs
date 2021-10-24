using Microsoft.AspNetCore.Mvc;

namespace MVC.Controllers
{
    [ApiController]
    public class TestController : ControllerBase
    {
        [HttpGet("/test")]
        public object Test()
        {
            return new
            {
                result = "success!"
            };
        }

        [HttpPost("/test/{id}")]
        public object GetBlog(int id, [FromQuery] string v, [FromBody] TestRequest request)
        {
            return new
            {
                result = $"{request.Title}_{id}_{v}"
            };
        }

        public class TestRequest
        {
            public string Title { get; set; }
        }
    }
}