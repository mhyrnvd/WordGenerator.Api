using Microsoft.AspNetCore.Mvc;
using WordGenerator.Api.Application.DTOs;
using WordGenerator.Api.Application.Services;

namespace WordGenerator.Api.Controllers
{
    [ApiController]
    [Route("api/excel")]
    public class ExcelController : ControllerBase
    {
        private readonly ExcelToTableService _excelService;

        public ExcelController(ExcelToTableService excelService)
        {
            _excelService = excelService;
        }

        [HttpPost("import")]
        public IActionResult Import([FromBody] ExcelImportDto dto)
        {
            try
            {
                var result = _excelService.ConvertExcelToTable(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = ex.Message });
            }
        }

        [HttpPost("preview")]
        public IActionResult Preview([FromBody] ExcelImportDto dto)
        {
            try
            {
                var result = _excelService.GetExcelPreview(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = ex.Message });
            }
        }

        [HttpPost("sheets")]
        public IActionResult GetSheets([FromBody] ExcelImportDto dto)
        {
            try
            {
                var result = _excelService.GetSheetNames(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { title = ex.Message });
            }
        }
    }
}