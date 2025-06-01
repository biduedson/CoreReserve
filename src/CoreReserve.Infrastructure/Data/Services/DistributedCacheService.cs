using System.Text;
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.Extensions;
using CoreReserve.Core.SharedKernel;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CoreReserve.Infrastructure.Data.Services
{
    /// <summary>
    /// Serviço de cache distribuído, responsável por armazenar e recuperar dados do cache.
    /// Implementa um mecanismo eficiente de recuperação e remoção de dados do cache distribuído.
    /// </summary>
    internal class DistributedCacheService(
        IDistributedCache distributedCache,
        ILogger<DistributedCacheService> logger,
        IOptions<CacheOptions> cacheOptions) : ICacheService
    {
        private const string CacheServiceName = nameof(DistributedCacheService);

        /// <summary>
        /// Define opções padrão para expiração do cache.
        /// Configuração baseada nas opções fornecidas via <see cref="CacheOptions"/>.
        /// </summary>
        private readonly DistributedCacheEntryOptions _cacheOptions = new()
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
            var valueBytes = await distributedCache.GetAsync(cacheKey);
            if (valueBytes?.Length > 0)
            {
                logger.LogInformation("----- Recuperado do {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);

                var value = Encoding.UTF8.GetString(valueBytes);
                return value.FromJson<TItem>();
            }

            var item = await factory();
            if (!item.IsDefault()) // SonarQube Bug: item != null
            {
                logger.LogInformation("----- Adicionado ao {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);

                var value = Encoding.UTF8.GetBytes(item.ToJson());
                await distributedCache.SetAsync(cacheKey, value, _cacheOptions);
            }

            return item;
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
            var valueBytes = await distributedCache.GetAsync(cacheKey);
            if (valueBytes?.Length > 0)
            {
                logger.LogInformation("----- Recuperado do {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);

                var values = Encoding.UTF8.GetString(valueBytes);
                return values.FromJson<IReadOnlyList<TItem>>();
            }

            var items = await factory();
            if (items?.Any() == true)
            {
                logger.LogInformation("----- Adicionado ao {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);

                var value = Encoding.UTF8.GetBytes(items.ToJson());
                await distributedCache.SetAsync(cacheKey, value, _cacheOptions);
            }

            return items;
        }

        /// <summary>
        /// Remove um ou mais itens do cache distribuído.
        /// </summary>
        /// <param name="cacheKeys">Lista de chaves a serem removidas do cache.</param>
        public async Task RemoveAsync(params string[] cacheKeys)
        {
            foreach (var cacheKey in cacheKeys)
            {
                logger.LogInformation("----- Removido do {CacheServiceName}: '{CacheKey}'", CacheServiceName, cacheKey);
                await distributedCache.RemoveAsync(cacheKey);
            }
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe DistributedCacheService → Implementa um serviço de cache distribuído para otimizar acesso e armazenamento temporário de dados. 
    ✅ Métodos GetOrCreateAsync() → Buscam itens no cache, adicionando-os caso ainda não estejam armazenados. 
    ✅ Método RemoveAsync() → Permite a remoção de múltiplos itens do cache baseado na chave. 
    ✅ Uso de ILogger → Garante log detalhado para monitoramento de operações realizadas no cache. 
    ✅ Configuração de expiração do cache via DistributedCacheEntryOptions → Define políticas de validade dos dados armazenados. 
    ✅ Essa abordagem melhora significativamente a performance e escalabilidade da aplicação, reduzindo carga em consultas ao banco. 
    */
}