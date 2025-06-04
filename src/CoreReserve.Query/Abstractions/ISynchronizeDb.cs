using System.Linq.Expressions;

namespace CoreReserve.Query.Abstractions
{
    /// <summary>
    /// Interface para sincronização de modelos de consulta com o banco de dados.
    /// Define operações para inserção, atualização e remoção de registros na base de consultas.
    /// </summary>
    public interface ISynchronizeDb : IDisposable
    {
        /// <summary>
        /// Insere ou atualiza um modelo de consulta no banco de dados.
        /// Se um registro correspondente ao filtro existir, ele será atualizado; caso contrário, um novo será criado.
        /// </summary>
        /// <typeparam name="TQueryModel">Tipo do modelo de consulta.</typeparam>
        /// <param name="queryModel">Modelo de consulta a ser inserido ou atualizado.</param>
        /// <param name="upsertFilter">Expressão de filtro usada para determinar a condição de atualização.</param>
        /// <returns>Tarefa assíncrona representando a operação de upsert.</returns>
        Task UpsertAsync<TQueryModel>(TQueryModel queryModel, Expression<Func<TQueryModel, bool>> upsertFilter)
            where TQueryModel : IQueryModel;

        /// <summary>
        /// Exclui modelos de consulta do banco de dados com base no filtro especificado.
        /// Remove apenas os registros que correspondem à condição definida na expressão de filtro.
        /// </summary>
        /// <typeparam name="TQueryModel">Tipo do modelo de consulta.</typeparam>
        /// <param name="deleteFilter">Expressão de filtro usada para determinar quais registros devem ser excluídos.</param>
        /// <returns>Tarefa assíncrona representando a operação de exclusão.</returns>
        Task DeleteAsync<TQueryModel>(Expression<Func<TQueryModel, bool>> deleteFilter)
            where TQueryModel : IQueryModel;
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Interface ISynchronizeDb → Define um contrato para sincronização de modelos de consulta com o banco de dados.
✅ Herança de IDisposable → Permite liberar corretamente recursos associados ao banco, evitando vazamentos de memória.
✅ Método UpsertAsync() → Realiza inserção ou atualização de um modelo de consulta no banco, garantindo consistência dos dados.
✅ Método DeleteAsync() → Permite remoção seletiva de registros com base em um filtro específico.
✅ Uso de Expressão Lambda como filtro → Garante flexibilidade na definição de condições de upsert e exclusão.
✅ Arquitetura baseada em CQRS → Mantém separação entre leitura e escrita, garantindo escalabilidade e eficiência no acesso aos dados.
✅ Essa abordagem melhora a integridade dos dados, reduz inconsistências e otimiza consultas em aplicações distribuídas.
*/
