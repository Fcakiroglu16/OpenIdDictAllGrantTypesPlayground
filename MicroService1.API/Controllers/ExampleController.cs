using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MicroService1.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExampleController : ControllerBase
{
    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        var claim = User.Claims;
        return Ok("get");
    }

    [HttpPost]
    [Authorize("ReadPolicy")]
    public IActionResult Post()
    {
        return Ok("post");
    }

    [HttpPut]
    [Authorize("WriteOrReadPolicy")]
    public IActionResult Put()
    {
        return Ok("put");
    }

    [HttpDelete]
    [Authorize("AdminPolicy")]
    public IActionResult Delete()
    {
        return Ok("delete");
    }
}