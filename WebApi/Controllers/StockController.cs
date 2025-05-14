using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly IStockService _svc;

        public StockController(IStockService svc) => _svc = svc;


        [HttpPost("import")]
        public async Task<IActionResult> Import([FromBody] List<ImportStockDto> items)
        {
            if (items == null || items.Count == 0)
                return BadRequest(new { Message = "No stock items provided." });

            await _svc.ImportAsync(items);
            return Ok(new { Message = $"{items.Count} items imported." });
        }
    }
}
