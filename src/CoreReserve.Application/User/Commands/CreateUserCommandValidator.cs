using FluentValidation;

namespace CoreReserve.Application.User.Commands
{
    /// <summary>
    /// Validador para o comando <see cref="CreateUserCommand"/>.
    /// Define regras de validação para garantir que os dados fornecidos sejam corretos.
    /// </summary>
    public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
    {
        /// <summary>
        /// Configuração das regras de validação para criação de clientes.
        /// </summary>
        public CreateUserCommandValidator()
        {
            // O nome não pode estar vazio e deve ter no máximo 100 caracteres.
            RuleFor(command => command.Name)
                .NotEmpty()
                .MaximumLength(100);

            // O e-mail não pode estar vazio, deve ter no máximo 254 caracteres e precisa ser um e-mail válido.
            RuleFor(command => command.Email)
                .NotEmpty()
                .MaximumLength(254)
                .EmailAddress();
        }
    }
}