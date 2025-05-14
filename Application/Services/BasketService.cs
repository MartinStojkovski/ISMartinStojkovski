using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Services
{
    public class BasketService : IBasketService
    {
        private readonly IRepository<Product> _products;
        private readonly IRepository<Stock> _stock;

        public BasketService(
            IRepository<Product> products,
            IRepository<Stock> stock)
        {
            _products = products;
            _stock = stock;
        }

        public async Task<DiscountResultDto> CalculateDiscountAsync(IEnumerable<BasketItemDto> items)
        {
            var result = new DiscountResultDto();

            var productIds = items.Select(i => i.ProductId).ToList();
            var products = (await _products.GetAllAsync())
                                .Where(p => productIds.Contains(p.Id))
                                .ToDictionary(p => p.Id);
            var stocks = (await _stock.GetAllAsync())
                                .Where(s => productIds.Contains(s.ProductId))
                                .ToDictionary(s => s.ProductId);

            foreach (var line in items)
            {
                if (!products.TryGetValue(line.ProductId, out var prod))
                    throw new KeyNotFoundException($"Product {line.ProductId} not found.");

                if (!stocks.TryGetValue(line.ProductId, out var stk)
                    || line.Quantity > stk.Quantity)
                {
                    throw new InvalidOperationException(
                        $"Not enough stock for '{prod.Name}'. Requested {line.Quantity}, available {stk?.Quantity ?? 0}.");
                }
            }

            foreach (var line in items)
            {
                var prod = products[line.ProductId];
                decimal unit = prod.Price;
                decimal disc = line.Quantity > 1
                              ? unit * 0.05m  
                              : 0m;
                decimal before = unit * line.Quantity;
                decimal total = before - disc;

                result.Lines.Add(new DiscountLineDto
                {
                    ProductId = prod.Id,
                    Name = prod.Name,
                    Quantity = line.Quantity,
                    UnitPrice = unit,
                    Discount = disc,
                    LineTotal = total
                });

                result.TotalBeforeDiscount += before;
                result.TotalDiscount += disc;
            }

            result.TotalAfterDiscount = result.TotalBeforeDiscount - result.TotalDiscount;
            return result;
        }
    }
}
