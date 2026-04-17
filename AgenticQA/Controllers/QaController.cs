using Microsoft.AspNetCore.Mvc;
using AgenticQA.Services;
using AgenticQA.Models;

namespace AgenticQA.Controllers
{
    [ApiController]
    [Route("api/qa")]
    public class QaController : ControllerBase
    {
        private readonly QaService _qaService;

        public QaController(QaService qaService)
        {
            _qaService = qaService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze(QaRequest req)
        {
            var result = await _qaService.Analyze(req.Query, req.Answer);
            return Ok(result);
        }
    }
}