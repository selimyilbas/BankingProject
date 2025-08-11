using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BankingApp.Domain.Interfaces
{
    /// <summary>
    /// Temel CRUD ve sorgu işlemleri için generic repository sözleşmesi.
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Kimliğe göre varlık getirir.
        /// </summary>
        Task<T?> GetByIdAsync(int id);
        /// <summary>
        /// Tüm varlıkları getirir.
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();
        /// <summary>
        /// Verilen koşula göre varlıkları arar.
        /// </summary>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression);
        /// <summary>
        /// Verilen koşula göre tek bir varlık getirir (yoksa null).
        /// </summary>
        Task<T?> SingleOrDefaultAsync(Expression<Func<T, bool>> expression);
        /// <summary>
        /// Yeni varlık ekler.
        /// </summary>
        Task AddAsync(T entity);
        /// <summary>
        /// Birden fazla varlık ekler.
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);
        /// <summary>
        /// Mevcut varlığı günceller.
        /// </summary>
        void Update(T entity);
        /// <summary>
        /// Varlığı siler.
        /// </summary>
        void Remove(T entity);
        /// <summary>
        /// Birden fazla varlığı siler.
        /// </summary>
        void RemoveRange(IEnumerable<T> entities);
        /// <summary>
        /// IQueryable olarak sorgu başlatır.
        /// </summary>
        IQueryable<T> Query();
    }
}
