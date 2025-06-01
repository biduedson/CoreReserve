
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.Extensions;
using CoreReserve.Core.SharedKernel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
namespace CoreReserve.Infrastructure.Data.Services
{
    /// <summary>
    /// Serviço de cache em memória, responsável por armazenar e recuperar dados temporários.
    /// Implementa um mecanismo eficiente de recuperação e remoção de dados do cache interno.
    /// </summary>
    internal class MemoryCacheService(
        ILogger<MemoryCacheService> logger,
        IMemoryCache memoryCache,
        IOptions<CacheOptions> cacheOptions) : ICacheService
    {
        private const string CacheServiceName = nameof(MemoryCacheService);

        /// <summary>
        /// Define opções padrão para expiração do cache.
        /// Configuração baseada nas opções fornecidas via <see cref="CacheOptions"/>.
        /// </summary>
        private readonly MemoryCacheEntryOptions _cacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(cacheOptions.Value.AbsoluteExpirationInHours),
            SlidingExpiration = TimeSpan.FromSeconds(cacheOptions.Value.SlidingExpirationInSeconds)
        };

        /// <summary>
        /// Obtém um item do cache ou o cria caso não exista.
        /// </summary>
        /// <typeparam name="TItem">Tipo do item armazenado.</typeparam>
        /// <param name="cacheKey">Chave única do cache.</param>
        /// <param name="factory">Função que gera o item caso não esteja no cache.</param>
        /// <returns>O item recuperado ou criado.</returns>
        public async Task<TItem> GetOrCreateAsync<TItem>(string cacheKey, Func<Task<TItem>> factory)
        {
            return await memoryCache.GetOrCreateAsync(cacheKey, async cacheEntry =>
            {
                var cacheValue = cacheEntry?.Value;
                if (cacheValue != null)
                {
                    logger.LogInformation("----- Recuperado do {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);
                    return (TItem)cacheValue;
                }

                var item = await factory();
                if (!item.IsDefault()) // SonarQube Bug: item != null
                {
                    logger.LogInformation("----- Adicionado ao {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);
                    memoryCache.Set(cacheKey, item, _cacheOptions);
                }

                return item;
            });
        }

        /// <summary>
        /// Obtém uma lista de itens do cache ou a cria caso não exista.
        /// </summary>
        /// <typeparam name="TItem">Tipo dos itens armazenados.</typeparam>
        /// <param name="cacheKey">Chave única do cache.</param>
        /// <param name="factory">Função que gera os itens caso não estejam no cache.</param>
        /// <returns>A lista de itens recuperada ou criada.</returns>
        public async Task<IReadOnlyList<TItem>> GetOrCreateAsync<TItem>(
            string cacheKey,
            Func<Task<IReadOnlyList<TItem>>> factory)
        {
            return await memoryCache.GetOrCreateAsync(cacheKey, async cacheEntry =>
            {
                var cacheValues = cacheEntry?.Value;
                if (cacheValues != null)
                {
                    logger.LogInformation("----- Recuperado do {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);
                    return (IReadOnlyList<TItem>)cacheValues;
                }

                var items = await factory();
                if (items?.Any() == true)
                {
                    logger.LogInformation("----- Adicionado ao {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);
                    memoryCache.Set(cacheKey, items, _cacheOptions);
                }

                return items;
            });
        }

        /// <summary>
        /// Remove um ou mais itens do cache em memória.
        /// </summary>
        /// <param name="cacheKeys">Lista de chaves a serem removidas do cache.</param>
        public Task RemoveAsync(params string[] cacheKeys)
        {
            foreach (var cacheKey in cacheKeys)
            {
                logger.LogInformation("----- Removido do {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);
                memoryCache.Remove(cacheKey);
            }

            return Task.CompletedTask;
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe MemoryCacheService → Implementa um serviço de cache em memória para otimizar acesso e armazenamento temporário de dados. 
    ✅ Métodos GetOrCreateAsync() → Buscam itens no cache, adicionando-os caso ainda não estejam armazenados. 
    ✅ Método RemoveAsync() → Permite a remoção de múltiplos itens do cache baseado na chave. 
    ✅ Uso de ILogger → Garante log detalhado para monitoramento de operações realizadas no cache. 
    ✅ Configuração de expiração do cache via MemoryCacheEntryOptions → Define políticas de validade dos dados armazenados. 
    ✅ Essa abordagem melhora significativamente a performance e escalabilidade da aplicação, reduzindo carga em consultas ao banco. 
    */

}