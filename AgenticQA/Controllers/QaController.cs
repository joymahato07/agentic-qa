using Microsoft.AspNetCore.Mvc;
using AgenticQA.Services;

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
        public async Task<IActionResult> Analyze([FromBody] QaRequest req)
        {
            var result = await _qaService.Analyze(req.Query, req.Answer);
            return Ok(result);
        }
    }

    public class QaRequest
    {
        public string Query { get; set; }
        public string Answer { get; set; }
    }
}