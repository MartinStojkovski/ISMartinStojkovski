using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

public class StockService : IStockService
{
    private readonly IRepository<Category> _catRepo;
    private readonly IRepository<Product> _prodRepo;
    private readonly IRepository<Stock> _stockRepo;
    private readonly IMapper _mapper;

    public StockService(
        IRepository<Category> catRepo,
        IRepository<Product> prodRepo,
        IRepository<Stock> stockRepo,
        IMapper mapper)
    {
        _catRepo = catRepo;
        _prodRepo = prodRepo;
        _stockRepo = stockRepo;
        _mapper = mapper;
    }

    public async Task ImportAsync(IEnumerable<ImportStockDto> items)
    {
        var existingCats = (await _catRepo.GetAllAsync()).ToList();
        var existingProds = (await _prodRepo.GetAllAsync()).ToList();
        var existingStk = (await _stockRepo.GetAllAsync()).ToList();

        foreach (var dto in items)
        {
            var dtoCatEntities = new List<Category>();
            foreach (var catName in dto.Categories)
            {
                var cat = existingCats
                    .FirstOrDefault(c => c.Name == catName);
                if (cat == null)
                {
                    cat = new Category(catName, null);
                    await _catRepo.AddAsync(cat);
                    await _catRepo.SaveChangesAsync();
                    existingCats.Add(cat);
                }
                dtoCatEntities.Add(cat);
            }

            var prod = existingProds
                .FirstOrDefault(p => p.Name == dto.Name);
            if (prod == null)
            {
                prod = new Product
                {
                    Name = dto.Name,
                    Description = null,
                    Price = dto.Price,
                    CategoryId = dtoCatEntities.First().Id
                };
                await _prodRepo.AddAsync(prod);
                await _prodRepo.SaveChangesAsync();
                existingProds.Add(prod);
            }

            var stock = existingStk
                .FirstOrDefault(s => s.ProductId == prod.Id);
            if (stock == null)
            {
                stock = new Stock
                {
                    ProductId = prod.Id,
                    Quantity = dto.Quantity,
                    LastUpdated = DateTime.UtcNow
                };
                await _stockRepo.AddAsync(stock);
            }
            else
            {
                stock.Quantity += dto.Quantity;
                _stockRepo.Update(stock);
            }
            await _stockRepo.SaveChangesAsync();
            if (!existingStk.Contains(stock))
                existingStk.Add(stock);
        }
    }
}
