using Microsoft.AspNetCore.Mvc;

namespace CoreTestingWebApp.Controllers
{
    [Route("test")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly HelloWorldService helloWorldService;
        private readonly GoodbyeWorldService goodbyeWorldService;
        public TestController(HelloWorldService hws, GoodbyeWorldService gws)
        {
            this.helloWorldService = hws;
            this.goodbyeWorldService = gws;
        }
        [HttpGet("test1")]
        public bool TestOne()
        {
            goodbyeWorldService.Echo("from inside the goodbye world service!");
            return true;
        }
    }
}