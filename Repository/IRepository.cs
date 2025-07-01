﻿namespace Proyect_InvOperativa.Repository
{
    public interface IRepository <T> where T : class
    {
        Task<T> GetByIdAsync(long id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteIdAsync(long id);
    }
}
