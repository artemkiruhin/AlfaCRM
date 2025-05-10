using AlfaCRM.Domain.Interfaces.Services.Security;
using AlfaCRM.Infrastructure;
using AlfaCRM.Services.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlfaCRM.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IHashService _hasher;

        public TestController(AppDbContext context, IHashService hasher)
        {
            _context = context;
            _hasher = hasher;
        }
        
        [HttpPost("init")]
        public IActionResult InitDatabase()
        {
            try
            {
                var result = DbInitializer.Initialize(_context, _hasher);
                return Ok(new {meg = result});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }
    }
}
