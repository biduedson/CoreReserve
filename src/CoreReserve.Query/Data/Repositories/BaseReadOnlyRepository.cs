using CoreReserve.Query.Abstractions;
using MongoDB.Driver;

namespace CoreReserve.Query.Data.Repositories
{
    /// <summary>
    /// Classe base para repositórios somente leitura.
    /// Responsável por fornecer operações de consulta em um banco NoSQL (MongoDB).
    /// </summary>
    /// <typeparam name="TQueryModel">Tipo do modelo de consulta.</typeparam>
    /// <typeparam name="TKey">Tipo da chave identificadora.</typeparam>
    internal abstract class BaseReadOnlyRepository<TQueryModel, TKey>(IReadDbContext context) : IReadOnlyRepository<TQueryModel, TKey>
        where TQueryModel : IQueryModel<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Coleção MongoDB que armazena os modelos de consulta.
        /// </summary>
        protected readonly IMongoCollection<TQueryModel> Collection = context.GetCollection<TQueryModel>();

        /// <summary>
        /// Obtém um modelo de consulta pelo identificador único.
        /// </summary>
        /// <param name="id">O identificador do modelo de consulta.</param>
        /// <returns>O modelo de consulta correspondente, ou nulo se não encontrado.</returns>
        public async Task<TQueryModel> GetByIdAsync(TKey id)
        {
            using var asyncCursor = await Collection.FindAsync(queryModel => queryModel.Id.Equals(id));
            return await asyncCursor.FirstOrDefaultAsync();
        }
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe BaseReadOnlyRepository → Representa um repositório genérico para operações de leitura em MongoDB.
✅ Herança de IReadOnlyRepository<TQueryModel, TKey> → Define operações genéricas de leitura sem modificação dos dados.
✅ Utilização de MongoDB → A classe usa MongoDB como banco de dados NoSQL para armazenar e recuperar informações.
✅ Propriedade Collection → Representa a coleção MongoDB que mantém os modelos de consulta.
✅ Método GetByIdAsync() → Recupera um registro específico da base de dados usando seu identificador único.
✅ Uso de TKey como IEquatable<TKey> → Garante que a chave implementa comparação de igualdade, melhorando integridade dos dados.
✅ Arquitetura baseada em CQRS → Mantém separação entre comandos (modificação de dados) e consultas (leitura de dados).
✅ Essa estrutura melhora desempenho, escalabilidade e flexibilidade da aplicação ao utilizar operações assíncronas e banco NoSQL.
*/
