using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _ctx;
        private readonly DbSet<T> _db;

        public Repository(AppDbContext ctx)
        {
            _ctx = ctx;
            _db = ctx.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync() =>
            await _db.ToListAsync();

        public async Task<T?> GetByIdAsync(Guid id) =>
            await _db.FindAsync(id);

        public async Task AddAsync(T entity) =>
            await _db.AddAsync(entity);

        public void Update(T entity) =>
            _db.Update(entity);

        public void Remove(T entity) =>
            _db.Remove(entity);

        public Task<int> SaveChangesAsync() =>
            _ctx.SaveChangesAsync();
    }
}
