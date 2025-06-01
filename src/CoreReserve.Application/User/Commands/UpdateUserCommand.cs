using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using MediatR;

namespace CoreReserve.Application.User.Commands
{
    /// <summary>
    /// Comando para atualizar as informações de um cliente.
    /// Contém o identificador único do cliente e o novo endereço de e-mail.
    /// </summary>
    public class UpdateUserCommand : IRequest<Result>
    {
        /// <summary>
        /// Identificador único do cliente que será atualizado.
        /// </summary>
        [Required]
        public Guid Id { get; set; }

        /// <summary>
        /// Novo endereço de e-mail do cliente.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
// -----------------------------------------
/*
✅ Classe UpdateUserCommand → Representa um comando para atualização de informações de um cliente. 
✅ Propriedade Id → Define o identificador único do cliente a ser atualizado. 
✅ Propriedade Email → Define o novo e-mail do cliente, garantindo que siga regras de validação. 
✅ Uso de Data Annotations → Garante que os dados fornecidos estejam em um formato válido antes do processamento. 
✅ Implementação de IRequest<Result> → Indica que o comando retornará um resultado padronizado de sucesso ou erro. 
✅ Essa estrutura melhora a integridade dos dados ao garantir que apenas clientes válidos sejam atualizados corretamente. 
*/