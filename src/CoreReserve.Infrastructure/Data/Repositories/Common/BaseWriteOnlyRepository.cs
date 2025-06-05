using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Infrastructure.Data.Context;


namespace CoreReserve.Infrastructure.Data.Repositories.Common
{
    /// <summary>
    /// Classe base para repositórios somente de escrita.
    /// </summary>
    /// <typeparam name="TEntity">O tipo da entidade.</typeparam>
    /// <typeparam name="TKey">O tipo da chave da entidade.</typeparam>
    internal abstract class BaseWriteOnlyRepository<TEntity, TKey>(WriteDbContext dbContext) : IWriteOnlyRepository<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Consulta compilada para busca de entidades por ID de forma assíncrona.
        /// Melhora a performance reduzindo a sobrecarga das consultas repetitivas.
        /// </summary>
        private static readonly Func<WriteDbContext, TKey, Task<TEntity>> GetByIdCompiledAsync =
            EF.CompileAsyncQuery((WriteDbContext dbContext, TKey id) =>
                dbContext
                    .Set<TEntity>()
                    .AsNoTrackingWithIdentityResolution()
                    .FirstOrDefault(entity => entity.Id.Equals(id)))!;

        private readonly DbSet<TEntity> _dbSet = dbContext.Set<TEntity>();
        protected readonly WriteDbContext DbContext = dbContext;

        /// <summary>
        /// Adiciona uma entidade ao contexto de banco de dados.
        /// </summary>
        /// <param name="entity">A entidade a ser adicionada.</param>
        public void Add(TEntity entity) =>
            _dbSet.Add(entity);

        /// <summary>
        /// Atualiza uma entidade existente no contexto de banco de dados.
        /// </summary>
        /// <param name="entity">A entidade a ser atualizada.</param>
        public void Update(TEntity entity) =>
            _dbSet.Update(entity);

        /// <summary>
        /// Remove uma entidade do contexto de banco de dados.
        /// </summary>
        /// <param name="entity">A entidade a ser removida.</param>
        public void Remove(TEntity entity) =>
            _dbSet.Remove(entity);

        /// <summary>
        /// Obtém uma entidade pelo ID de forma assíncrona utilizando consulta compilada.
        /// </summary>
        /// <param name="id">O ID da entidade.</param>
        /// <returns>A entidade correspondente ao ID informado.</returns>
        public async Task<TEntity> GetByIdAsync(TKey id) =>
            await GetByIdCompiledAsync(DbContext, id);

        #region IDisposable

        // Para detectar chamadas redundantes.
        private bool _disposed;

        ~BaseWriteOnlyRepository() => Dispose(false);

        /// <summary>
        /// Implementação pública do padrão Dispose, chamável por consumidores.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementação protegida do padrão Dispose.
        /// </summary>
        /// <param name="disposing">Define se a liberação de recursos está ocorrendo explicitamente.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            // Libera recursos gerenciados
            if (disposing)
                DbContext.Dispose();

            _disposed = true;
        }

        #endregion
    }
}