using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces
{
    public interface IStockService
    {
      
        Task ImportAsync(IEnumerable<ImportStockDto> items);
    }
}
