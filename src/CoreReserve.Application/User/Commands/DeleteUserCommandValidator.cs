using FluentValidation;

namespace CoreReserve.Application.User.Commands
{
    /// <summary>
    /// Validador para o comando <see cref="DeleteUserCommand"/>.
    /// Garante que o identificador do cliente seja válido antes da exclusão.
    /// </summary>
    public class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
    {
        /// <summary>
        /// Configuração das regras de validação para a exclusão de um cliente.
        /// </summary>
        public DeleteUserCommandValidator()
        {
            // O ID do cliente não pode estar vazio.
            RuleFor(command => command.Id)
                .NotEmpty();
        }
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
// -----------------------------------------
/*
✅ Classe DeleteUserCommandValidator → Define regras de validação para o comando DeleteUserCommand. 
✅ Uso de FluentValidation → Permite uma validação fluida para garantir integridade dos dados de entrada. 
✅ Regra de validação do ID → Garante que o identificador do cliente não seja um valor vazio antes da exclusão. 
✅ Essa abordagem evita erros e garante que apenas registros válidos sejam processados. 
*/