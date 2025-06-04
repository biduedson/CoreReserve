using Ardalis.Result;
using CoreReserve.Query.QueriesModel;
using MediatR;

namespace CoreReserve.Query.Application.User.Queries
{
    /// <summary>
    /// Consulta para obter um usuário específico pelo identificador único.
    /// Responsável por recuperar os dados de um usuário sem modificar registros.
    /// </summary>
    public class GetUserByIdQuery(Guid id) : IRequest<Result<UserQueryModel>>
    {
        /// <summary>
        /// Identificador único do usuário que será buscado.
        /// </summary>
        public Guid Id { get; } = id;
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe GetUserByIdQuery → Define uma consulta para recuperar um usuário específico armazenado no sistema.
✅ Herança de IRequest<Result<UserQueryModel>> → Utiliza MediatR para facilitar a comunicação entre camadas.
✅ Uso de Ardalis.Result → Encapsula a resposta da consulta, permitindo indicar sucesso ou erro de forma estruturada.
✅ Propriedade Id → Define o identificador do usuário que será consultado, garantindo que a busca seja precisa.
✅ Arquitetura baseada em CQRS → Mantém separação entre leitura (query) e escrita (command), garantindo escalabilidade e organização.
✅ Essa abordagem melhora a eficiência da recuperação de dados e permite fácil manutenção da lógica de negócios.
*/
