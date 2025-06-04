using System;
using System.Threading.Tasks;

namespace CoreReserve.Query.Abstractions
{
    /// <summary>
    /// Interface para repositório somente leitura.
    /// Responsável por fornecer acesso a modelos de consulta sem modificar dados no banco.
    /// </summary>
    /// <typeparam name="TQueryModel">Tipo do modelo de consulta.</typeparam>
    /// <typeparam name="TKey">Tipo da chave identificadora do modelo de consulta.</typeparam>
    public interface IReadOnlyRepository<TQueryModel, in TKey>
        where TQueryModel : IQueryModel<TKey>
        where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Obtém um modelo de consulta pelo identificador único de forma assíncrona.
        /// </summary>
        /// <param name="id">Identificador único do modelo de consulta.</param>
        /// <returns>Tarefa assíncrona representando a operação de busca, retornando o modelo de consulta.</returns>
        Task<TQueryModel> GetByIdAsync(TKey id);
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Interface IReadOnlyRepository → Define um contrato para acesso somente leitura a modelos de consulta.
✅ Uso de métodos genéricos → Permite manipulação de diferentes tipos de query models sem alterar a implementação base.
✅ Restrição where TKey : IEquatable<TKey> → Garante que a chave identificadora seja única e comparável.
✅ Método GetByIdAsync() → Recupera um modelo de consulta do banco de dados utilizando operações assíncronas.
✅ Arquitetura baseada em CQRS → Separa operações de leitura e escrita, garantindo escalabilidade e eficiência.
✅ Essa abordagem melhora a integridade dos dados, facilita consultas eficientes e evita modificações acidentais no banco.
*/
