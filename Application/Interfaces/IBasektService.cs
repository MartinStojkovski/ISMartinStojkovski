using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IBasketService
    {
        
        Task<DiscountResultDto> CalculateDiscountAsync(IEnumerable<BasketItemDto> items);
    }
}
