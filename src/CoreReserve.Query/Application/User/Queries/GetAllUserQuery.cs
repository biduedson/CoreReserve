using Ardalis.Result;
using CoreReserve.Query.QueriesModel;
using MediatR;

namespace CoreReserve.Query.Application.User.Queries
{
    /// <summary>
    /// Consulta para obter todos os usuarios.
    /// Responsável por solicitar ao sistema uma lista completa de usuarios sem modificar dados.
    /// </summary>
    public class GetAllUserQuery : IRequest<Result<IEnumerable<UserQueryModel>>>;
}


// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe GetAllCustomerQuery → Define uma consulta para recuperar todos os usuarios armazenados no sistema.
✅ Herança de IRequest<Result<IEnumerable<CustomerQueryModel>>> → Utiliza MediatR para facilitar a comunicação entre camadas.
✅ Uso de Ardalis.Result → Encapsula a resposta da consulta, permitindo indicar sucesso ou erro de forma estruturada.
✅ Arquitetura baseada em CQRS → Mantém separação entre leitura (query) e escrita (command), garantindo escalabilidade e organização.
✅ Essa abordagem melhora a eficiência da recuperação de dados e permite fácil manutenção da lógica de negócios.
*/

