using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketService _svc;

        public BasketController(IBasketService svc) => _svc = svc;

       
        [HttpPost("discount")]
        public async Task<IActionResult> Discount([FromBody] List<BasketItemDto> items)
        {
            if (items == null || items.Count == 0)
                return BadRequest(new { Message = "Basket is empty." });

            try
            {
                var result = await _svc.CalculateDiscountAsync(items);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { Message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
