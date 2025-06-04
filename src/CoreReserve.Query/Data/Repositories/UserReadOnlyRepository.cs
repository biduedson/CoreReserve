using CoreReserve.Query.Abstractions;
using CoreReserve.Query.Data.Repositories.Abstractions;
using CoreReserve.Query.QueriesModel;
using MongoDB.Driver;

namespace CoreReserve.Query.Data.Repositories
{
    /// <summary>
    /// Repositório somente leitura para usuários.
    /// Responsável por recuperar informações de usuários armazenadas em um banco NoSQL (MongoDB).
    /// </summary>
    internal class UserReadOnlyRepository(IReadDbContext readDbContext)
        : BaseReadOnlyRepository<UserQueryModel, Guid>(readDbContext), IUserReadOnlyRepository
    {
        /// <summary>
        /// Obtém todos os usuários ordenados por nome e data de nascimento.
        /// </summary>
        /// <returns>Uma coleção de <see cref="UserQueryModel"/>.</returns>
        public async Task<IEnumerable<UserQueryModel>> GetAllAsync()
        {
            // Define critérios de ordenação: Por nome (ascendente), depois por data de criaçao do usuario (descendente).
            var sort = Builders<UserQueryModel>.Sort
                .Ascending(user => user.Name)
                .Descending(user => user.CreatedAt);

            // Configura opções de busca no banco de dados.
            var findOptions = new FindOptions<UserQueryModel>
            {
                Sort = sort
            };

            // Executa a consulta para buscar todos os usuários, aplicando as opções definidas.
            using var asyncCursor = await Collection.FindAsync(Builders<UserQueryModel>.Filter.Empty, findOptions);
            return await asyncCursor.ToListAsync();
        }
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe UserReadOnlyRepository → Implementa um repositório para leitura de usuários no MongoDB.
✅ Herança de BaseReadOnlyRepository → Reutiliza lógica de acesso ao banco de consultas.
✅ Método GetAllAsync() → Recupera todos os usuários ordenados por nome e data de nascimento.
✅ Uso de Builders.Sort → Permite ordenação personalizada, garantindo que os dados sejam retornados de maneira organizada.
✅ Uso de Builders.Filter.Empty → Recupera todos os registros sem aplicar filtros específicos.
✅ Arquitetura baseada em CQRS → Separa operações de leitura e escrita, garantindo escalabilidade e eficiência na consulta de dados.
✅ Essa abordagem melhora a performance ao lidar com grandes volumes de dados e consultas complexas.
*/
