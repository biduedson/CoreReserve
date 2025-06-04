using MongoDB.Driver;

namespace CoreReserve.Query.Abstractions
{
    /// <summary>
    /// Representa o contexto do banco de dados somente leitura para consultas.
    /// Responsável por fornecer acesso a coleções do MongoDB sem modificar dados.
    /// </summary>
    public interface IReadDbContext : IDisposable
    {
        /// <summary>
        /// Obtém a string de conexão do banco de dados.
        /// </summary>
        string ConnectionString { get; }

        /// <summary>
        /// Obtém a coleção do banco de dados para o modelo de consulta especificado.
        /// </summary>
        /// <typeparam name="TQueryModel">Tipo do modelo de consulta.</typeparam>
        /// <returns>Coleção MongoDB contendo registros do tipo especificado.</returns>
        IMongoCollection<TQueryModel> GetCollection<TQueryModel>() where TQueryModel : IQueryModel;

        /// <summary>
        /// Cria coleções no banco de dados para todos os modelos de consulta existentes.
        /// </summary>
        /// <returns>Tarefa assíncrona representando a criação das coleções.</returns>
        Task CreateCollectionsAsync();
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Interface IReadDbContext → Define um contexto para consulta de dados no banco NoSQL.
✅ Herança de IDisposable → Permite que o contexto seja liberado corretamente, evitando vazamentos de memória.
✅ Propriedade ConnectionString → Fornece a string de conexão do banco, garantindo acesso aos dados.
✅ Método GetCollection<TQueryModel>() → Permite recuperar coleções de documentos do MongoDB com base no tipo de consulta.
✅ Método CreateCollectionsAsync() → Garante que todas as coleções necessárias sejam criadas no banco antes da execução das queries.
✅ Arquitetura baseada em CQRS → Mantém separação entre leitura e escrita, garantindo escalabilidade e organização.
✅ Essa abordagem melhora a integridade dos dados e facilita a manutenção do banco de consultas.
*/
