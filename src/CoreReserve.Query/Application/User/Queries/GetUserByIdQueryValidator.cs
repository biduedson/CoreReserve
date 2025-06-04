using FluentValidation;

namespace CoreReserve.Query.Application.User.Queries
{
    /// <summary>
    /// Validador para a consulta de obtenção de usuário por ID.
    /// Garante que a consulta contenha um identificador válido antes de ser processada.
    /// </summary>
    public class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
    {
        /// <summary>
        /// Configura as regras de validação para a consulta.
        /// </summary>
        public GetUserByIdQueryValidator()
        {
            RuleFor(command => command.Id)
                .NotEmpty() // Garante que o ID não esteja vazio.
                .WithMessage("User ID cannot be empty.");
        }
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe GetUserByIdQueryValidator → Valida a consulta antes de sua execução.
✅ Herança de AbstractValidator<GetUserByIdQuery> → Utiliza FluentValidation para definir regras de validação.
✅ Uso de RuleFor(command => command.Id) → Garante que o campo ID seja obrigatório antes de processar a consulta.
✅ Uso de .WithMessage() → Permite definir mensagens de erro personalizadas para melhorar a experiência do usuário.
✅ Arquitetura baseada em CQRS → Mantém separação entre leitura (query) e validação, garantindo organização e escalabilidade.
✅ Essa abordagem melhora a integridade dos dados e evita consultas inválidas.
*/
