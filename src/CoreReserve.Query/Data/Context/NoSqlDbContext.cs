using System.Linq.Expressions;
using System.Reflection;
using CoreReserve.Core.AppSettings;
using CoreReserve.Core.Extensions;
using CoreReserve.Query.Abstractions;
using CoreReserve.Query.QueriesModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Polly;
using Polly.Retry;

namespace CoreReserve.Query.Data.Context
{
    /// <summary>
    /// Contexto para acesso ao banco NoSQL (MongoDB).
    /// Responsável por gerenciar coleções, índices e operações de sincronização.
    /// </summary>
    public sealed class NoSqlDbContext : IReadDbContext, ISynchronizeDb
    {
        #region Construtor

        private const string DatabaseName = "CoreReseve";
        private const int RetryCount = 2;

        private static readonly ReplaceOptions DefaultReplaceOptions = new()
        {
            IsUpsert = true
        };

        private static readonly CreateIndexOptions DefaultCreateIndexOptions = new()
        {
            Unique = true,
            Sparse = true
        };

        private readonly MongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        private readonly ILogger<NoSqlDbContext> _logger;
        private readonly AsyncRetryPolicy _mongoRetryPolicy;

        /// <summary>
        /// Inicializa o contexto do banco de dados NoSQL.
        /// </summary>
        /// <param name="options">Opções de configuração do banco.</param>
        /// <param name="logger">Logger para rastreamento de erros.</param>
        public NoSqlDbContext(IOptions<ConnectionOptions> options, ILogger<NoSqlDbContext> logger)
        {
            ConnectionString = options.Value.NoSqlConnection;

            _mongoClient = new MongoClient(options.Value.NoSqlConnection);
            _mongoDatabase = _mongoClient.GetDatabase(DatabaseName);
            _logger = logger;
            _mongoRetryPolicy = CreateRetryPolicy(logger);
        }

        #endregion

        #region IReadDbContext

        /// <summary>
        /// String de conexão do banco de dados.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Obtém uma coleção do banco MongoDB.
        /// </summary>
        public IMongoCollection<TQueryModel> GetCollection<TQueryModel>() where TQueryModel : IQueryModel =>
            _mongoDatabase.GetCollection<TQueryModel>(typeof(TQueryModel).Name);

        /// <summary>
        /// Cria coleções no banco de dados MongoDB.
        /// </summary>
        public async Task CreateCollectionsAsync()
        {
            using var asyncCursor = await _mongoDatabase.ListCollectionNamesAsync();
            var collections = await asyncCursor.ToListAsync();

            foreach (var collectionName in GetCollectionNamesFromAssembly())
            {
                if (!collections.Exists(db => db.Equals(collectionName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _logger.LogInformation("----- MongoDB: criando a coleção {Name}", collectionName);

                    await _mongoDatabase.CreateCollectionAsync(collectionName, new CreateCollectionOptions
                    {
                        ValidationLevel = DocumentValidationLevel.Strict
                    });
                }
                else
                {
                    _logger.LogInformation("----- MongoDB: coleção {Name} já existe", collectionName);
                }
            }

            await CreateIndexAsync();
        }

        /// <summary>
        /// Cria índices no banco de dados.
        /// </summary>
        private async Task CreateIndexAsync()
        {
            _logger.LogInformation("----- MongoDB: criando índices...");

            var indexDefinition = Builders<UserQueryModel>.IndexKeys.Ascending(model => model.Email);
            var indexModel = new CreateIndexModel<UserQueryModel>(indexDefinition, DefaultCreateIndexOptions);
            var collection = GetCollection<UserQueryModel>();

            var indexName = await collection.Indexes.CreateOneAsync(indexModel);

            _logger.LogInformation("----- MongoDB: índices criados com sucesso - {indexName}", indexName);
        }

        private static List<string> GetCollectionNamesFromAssembly() =>
            [.. Assembly
            .GetExecutingAssembly()
            .GetAllTypesOf<IQueryModel>()
            .Select(impl => impl.Name)
            .Distinct()];

        #endregion

        #region ISynchronizeDb

        public async Task UpsertAsync<TQueryModel>(TQueryModel queryModel, Expression<Func<TQueryModel, bool>> upsertFilter)
            where TQueryModel : IQueryModel
        {
            var collection = GetCollection<TQueryModel>();
            await _mongoRetryPolicy.ExecuteAsync(async () => await collection.ReplaceOneAsync(upsertFilter, queryModel, DefaultReplaceOptions));
        }

        public async Task DeleteAsync<TQueryModel>(Expression<Func<TQueryModel, bool>> deleteFilter)
            where TQueryModel : IQueryModel
        {
            var collection = GetCollection<TQueryModel>();
            await _mongoRetryPolicy.ExecuteAsync(async () => await collection.DeleteOneAsync(deleteFilter));
        }

        private static AsyncRetryPolicy CreateRetryPolicy(ILogger logger) =>
            Policy.Handle<MongoException>()
                  .WaitAndRetryAsync(RetryCount, retryAttempt => SleepDurationProvider(retryAttempt, logger), (ex, _) => OnRetry(logger, ex));

        private static TimeSpan SleepDurationProvider(int retryAttempt, ILogger logger)
        {
            var sleepDuration = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
            logger.LogWarning("----- MongoDB: tentativa #{Count} com atraso {Delay}", retryAttempt, sleepDuration);
            return sleepDuration;
        }

        private static void OnRetry(ILogger logger, Exception ex) =>
            logger.LogError(ex, "Erro inesperado ao salvar no MongoDB: {Message}", ex.Message);

        #endregion

        #region IDisposable

        private bool _disposed;

        ~NoSqlDbContext() => Dispose(false);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _mongoClient.Dispose();

            _disposed = true;
        }

        #endregion
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------


/*🔹 Explicação Detalhada
✅ Classe NoSqlDbContext → Contexto de acesso ao MongoDB para leitura e sincronização de dados.
✅ Implementação de IReadDbContext e ISynchronizeDb → Permite operações de leitura e manipulação de dados.
✅ Uso de MongoClient → Gerencia conexão com o banco.
✅ Método CreateCollectionsAsync() → Garante que coleções necessárias sejam criadas.
✅ Método CreateIndexAsync() → Define índices para otimizar buscas por e-mail.
✅ Uso de Polly para retry → Implementa estratégia de retry com exponential backoff para evitar falhas em momentos de instabilidade.
✅ Uso de IDisposable → Garante liberação eficiente de recursos.*/
