using Microsoft.AspNetCore.Mvc;
using WordGenerator.Api.Application.Requests;
using WordGenerator.Api.Application.Services;

namespace WordGenerator.Api.Controllers
{
    [ApiController]
    [Route("api/documents")]
    public class DocumentController : ControllerBase
    {
        private readonly WordGeneratorService _wordService;

        public DocumentController(WordGeneratorService wordService)
        {
            _wordService = wordService;
        }

        [HttpPost("generate")]
        public async Task<IActionResult> Generate(GenerateDocumentRequest request)
        {
            var file = await _wordService.GenerateAsync(request);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                "document.docx"
            );
        }
    }
}
