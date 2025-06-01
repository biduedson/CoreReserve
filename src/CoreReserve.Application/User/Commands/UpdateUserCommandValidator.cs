using FluentValidation;

namespace CoreReserve.Application.User.Commands
{
    /// <summary>
    /// Validador para o comando <see cref="UpdateUserCommand"/>.
    /// Garante que os dados do cliente sejam válidos antes da atualização.
    /// </summary>
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        /// <summary>
        /// Configuração das regras de validação para atualização de clientes.
        /// </summary>
        public UpdateUserCommandValidator()
        {
            // O ID do cliente não pode estar vazio.
            RuleFor(command => command.Id)
                .NotEmpty();

            // O e-mail não pode estar vazio, deve ter no máximo 254 caracteres e precisa ser um e-mail válido.
            RuleFor(command => command.Email)
                .NotEmpty()
                .MaximumLength(254)
                .EmailAddress();
        }
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
// -----------------------------------------
/*
✅ Classe UpdateUserCommandValidator → Define regras de validação para o comando UpdateUserCommand. 
✅ Uso de FluentValidation → Permite uma validação fluida e extensível para garantir integridade dos dados. 
✅ Regra de validação do ID → Garante que o identificador do cliente esteja preenchido corretamente. 
✅ Regra de validação do Email → Impede e-mails inválidos, garantindo um formato válido e um tamanho adequado. 
✅ Essa abordagem evita erros, melhora a consistência dos dados e garante atualizações seguras no sistema. 
*/
