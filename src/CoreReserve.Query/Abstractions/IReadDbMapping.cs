namespace CoreReserve.Query.Abstractions
{
    /// <summary>
    /// Interface para configuração de mapeamentos de banco de dados.
    /// Responsável por definir como os dados serão lidos e interpretados no contexto de consultas.
    /// </summary>
    public interface IReadDbMapping
    {
        /// <summary>
        /// Configura os mapeamentos para leitura do banco de dados.
        /// Garante que as entidades sejam corretamente registradas e acessadas no sistema.
        /// </summary>
        void Configure();
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Interface IReadDbMapping → Define um contrato para configuração de mapeamentos de banco de dados.
✅ Método Configure() → Responsável por ajustar as convenções e registros no contexto de leitura.
✅ Arquitetura baseada em CQRS → Mantém separação entre leitura e escrita, garantindo escalabilidade e organização.
✅ Essa abordagem melhora a estrutura dos dados e facilita consultas eficientes dentro do sistema.
*/
