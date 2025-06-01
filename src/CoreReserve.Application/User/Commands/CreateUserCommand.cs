using System.ComponentModel.DataAnnotations;
using Ardalis.Result;
using CoreReserve.Application.User.Responses;
using CoreReserve.Domain.Entities.UserAggregate;
using MediatR;

namespace CoreReserve.Application.User.Commands
{
    public class CreateUserCommand : IRequest<Result<CreatedUserResponse>>
    {
        /// <summary>
        /// Comando para criar um novo cliente.
        /// Contém as propriedades necessárias para o cadastro e segue validações adequadas.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        /// <summary>
        /// Gênero do cliente.
        /// </summary>
        public EGender Gender { get; set; }

        /// <summary>
        /// Endereço de e-mail do cliente.
        /// </summary>
        [Required]
        [MaxLength(200)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        /// <summary>
        /// Password do cliente.
        /// </summary>
        [Required]
        [MinLength(6)]
        [MaxLength(20)]
        public string Password { get; set; }

        /// <summary>
        /// Data de criação do cliente.
        /// </summary>
        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
// -----------------------------------------
/*
✅ Classe CreateUserCommand → Representa um comando para criar um novo cliente na aplicação. 
✅ Implementação de IRequest<Result<CreatedUserResponse>> → Indica que o comando retorna um resultado tipado com um response. 
✅ Validações com Data Annotations → Garante que os dados inseridos são válidos antes do processamento. 
✅ Uso de Ardalis.Result → Facilita o gerenciamento do resultado, permitindo respostas padronizadas. 
✅ Essa estrutura melhora a confiabilidade da entrada de dados e padroniza a criação de clientes dentro da aplicação. 
*/