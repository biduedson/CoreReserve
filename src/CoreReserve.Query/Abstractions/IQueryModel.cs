using System;

namespace CoreReserve.Query.Abstractions
{
    /// <summary>
    /// Representa a interface base para modelos de consulta.
    /// Utilizado como contrato para identificar objetos que fazem parte da camada de leitura.
    /// </summary>
    public interface IQueryModel;

    /// <summary>
    /// Interface genérica para um modelo de consulta com chave identificadora.
    /// Garante que cada entidade de consulta possua um identificador único do tipo especificado.
    /// </summary>
    /// <typeparam name="TKey">Tipo da chave identificadora.</typeparam>
    public interface IQueryModel<out TKey> : IQueryModel where TKey : IEquatable<TKey>
    {
        /// <summary>
        /// Obtém o identificador único do modelo de consulta.
        /// </summary>
        TKey Id { get; }
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Interface IQueryModel → Define um contrato comum para todos os modelos de consulta.
✅ Interface IQueryModel<TKey> → Define uma interface genérica para modelos que possuem um identificador único.
✅ Uso de out TKey → Garante que o tipo da chave seja apenas de saída, impedindo modificações internas.
✅ Restrição where TKey : IEquatable<TKey> → Exige que a chave implementa comparação de igualdade para garantir integridade dos dados.
✅ Arquitetura baseada em CQRS → Mantém separação entre leitura (query) e escrita (command), garantindo escalabilidade e organização.
✅ Essa abordagem melhora a padronização e permite consultas eficientes sem risco de modificação acidental.
*/
