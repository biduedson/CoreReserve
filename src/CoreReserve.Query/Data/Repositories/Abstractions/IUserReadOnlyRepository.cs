using CoreReserve.Query.Abstractions;
using CoreReserve.Query.QueriesModel;

namespace CoreReserve.Query.Data.Repositories.Abstractions
{
    /// <summary>
    /// Interface de repositório somente leitura para clientes.
    /// Define operações que podem ser executadas sem modificar os dados.
    /// </summary>
    public interface IUserReadOnlyRepository : IReadOnlyRepository<UserQueryModel, Guid>
    {
        /// <summary>
        /// Obtém todos os clientes disponíveis no banco de consultas.
        /// </summary>
        /// <returns>Uma coleção de <see cref="CustomerQueryModel"/>.</returns>
        Task<IEnumerable<UserQueryModel>> GetAllAsync();
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Interface ICustomerReadOnlyRepository → Define um repositório para consultas de clientes sem modificação dos dados.
✅ Herança de IReadOnlyRepository<CustomerQueryModel, Guid> → Reutiliza métodos genéricos para manipulação de dados de consulta.
✅ Método GetAllAsync() → Retorna todos os registros de clientes no banco de consultas de forma assíncrona.
✅ Uso de Task<IEnumerable<T>> → Garante que a operação seja executada de forma eficiente, sem bloquear a aplicação.
✅ Arquitetura baseada em CQRS → Separa operações de leitura e escrita, melhorando desempenho e escalabilidade.
✅ Essa abordagem melhora a consulta de dados, garantindo que apenas operações seguras sejam realizadas no repositório.
*/
