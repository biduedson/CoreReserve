using CoreReserve.Core.SharedKernel;

namespace CoreReserve.Application.User.Responses
{
    /// <summary>
    /// Representa a resposta retornada após a criação de um cliente.
    /// Contém apenas o identificador do cliente recém-criado.
    /// </summary>
    public class CreatedUserResponse(Guid id) : IResponse
    {
        /// <summary>
        /// Obtém o identificador único do cliente recém-criado.
        /// </summary>
        public Guid Id { get; } = id;
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe CreatedCustomerResponse → Define a resposta retornada após a criação de um cliente. 
    ✅ Propriedade Id → Armazena o identificador único (GUID) do cliente recém-criado. 
    ✅ Uso do construtor primário → Simplifica a inicialização da classe. 
    ✅ Implementação de IResponse → Indica que essa classe representa um tipo de resposta padrão da aplicação. 
    ✅ Essa abordagem fornece uma resposta limpa e objetiva ao usuário após o registro de um novo cliente. 
    */

}