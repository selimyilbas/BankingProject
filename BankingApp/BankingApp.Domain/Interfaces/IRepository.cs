using System.Linq.Expressions;

namespace BankingApp.Domain.Interfaces
{
    /// <summary>
    /// Basit CRUD işlemleri için repository sözleşmesi.
    /// </summary>
    public interface IRepository<T> where T : class
    {
        /// <summary>
        /// Kimliğe göre varlık getirir.
        /// </summary>
        Task<T?> GetByIdAsync(int id);
        /// <summary>
        /// Tüm varlıkları getirir.
        /// </summary>
        Task<List<T>> GetAllAsync();
        /// <summary>
        /// Koşula göre varlıkları arar.
        /// </summary>
        Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Yeni varlık ekler.
        /// </summary>
        Task AddAsync(T entity);
        /// <summary>
        /// Varlığı günceller.
        /// </summary>
        void Update(T entity);
        /// <summary>
        /// Varlığı siler.
        /// </summary>
        void Remove(T entity);
        /// <summary>
        /// IQueryable olarak sorgu başlatır.
        /// </summary>
        IQueryable<T> Query(); 
    }
}
