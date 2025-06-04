using Ardalis.Result;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.ValueObjects;

namespace CoreReserve.Domain.Factories;

/// <summary>
/// Fábrica para criação de usuários.
/// Responsável por validar e instanciar objetos da entidade User.
/// </summary>
public static class UserFactory
{
    /// <summary>
    /// Cria um usuário validando os dados antes da instanciação.
    /// Se houver erros, retorna um resultado com mensagens de erro.
    /// </summary>
    /// <param name="name">Nome do usuário.</param>
    /// <param name="gender">Gênero do usuário.</param>
    /// <param name="email">Endereço de e-mail do usuário.</param>
    /// <param name="password">Senha do usuário.</param>
    /// <param name="createdAt">Data de criação do usuário.</param>
    /// <returns>Resultado contendo o usuário criado ou mensagens de erro.</returns>
    public static Result<User> Create(
        string name,
        EGender gender,
        string email,
        string password,
        DateTime createdAt
    )
    {
        var emailResult = Email.Create(email);
        var passwordResult = Password.Create(password);
        var errors = new List<string>();

        if (!emailResult.IsSuccess)
            errors.AddRange(emailResult.Errors);

        if (!passwordResult.IsSuccess)
            errors.AddRange(passwordResult.Errors);

        // Se houver erros na validação, retorna um resultado com mensagens de erro.
        return errors.Any() ? Result<User>.Error(new ErrorList(errors.ToArray())) :
            Result<User>.Success(new User(name, gender, emailResult.Value, passwordResult.Value, createdAt));
    }

    /// <summary>
    /// Cria um usuário sem necessidade de validação dos dados.
    /// Assume que email e senha já foram validados externamente.
    /// </summary>
    /// <param name="name">Nome do usuário.</param>
    /// <param name="gender">Gênero do usuário.</param>
    /// <param name="email">Objeto de e-mail validado.</param>
    /// <param name="password">Objeto de senha validado.</param>
    /// <param name="createdAt">Data de criação do usuário.</param>
    /// <returns>Usuário criado sem realizar validações internas.</returns>
    public static User Create(string name, EGender gender, Email email, Password password, DateTime createdAt)
        => new(name, gender, email, password, createdAt);
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe UserFactory → Implementa o padrão Factory para criação de usuários.
✅ Método Create() com validação → Usa Ardalis.Result para encapsular erros e garantir que e-mail e senha sejam válidos antes de instanciar um usuário.
✅ Método Create() sem validação → Assume que os objetos `Email` e `Password` já foram validados, permitindo a instanciação direta.
✅ Uso de Email.Create() e Password.Create() → Aplica regras de validação antes da criação de um novo usuário.
✅ Uso de Ardalis.Result → Encapsula o resultado e permite tratamento estruturado de erros.
✅ Arquitetura baseada em Domain-Driven Design → Mantém separação entre entidades e lógica de criação de objetos.
✅ Essa abordagem melhora a integridade dos dados e facilita manutenção e testes do sistema.
*/