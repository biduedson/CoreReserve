using Ardalis.Result;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.ValueObjects;

namespace CoreReserve.Domain.Factories;

/// <summary>
/// Fábrica para criação de usuários.
/// Responsável por validar e instanciar objetos da entidade <see cref="User"/>.
/// </summary>
public static class UserFactory
{
    /// <summary>
    /// Cria um usuário após validar os dados fornecidos.
    /// Se houver erros na validação, retorna uma lista de mensagens de erro.
    /// </summary>
    /// <param name="name">Nome completo do usuário.</param>
    /// <param name="gender">Gênero do usuário.</param>
    /// <param name="email">Endereço de e-mail do usuário.</param>
    /// <param name="password">Senha de acesso do usuário.</param>
    /// <param name="createdAt">Data e hora de criação do usuário.</param>
    /// <returns>
    /// Um <see cref="Result{T}"/> contendo o usuário criado caso a validação tenha sucesso,
    /// ou uma lista de erros caso os dados sejam inválidos.
    /// </returns>
    public static Result<User> Create(
        string name,
        EGender gender,
        string email,
        string password
    )
    {
        var emailResult = Email.Create(email);
        var passwordResult = Password.Create(password);
        var errors = new List<string>();

        if (!emailResult.IsSuccess)
            errors.AddRange(emailResult.Errors);

        if (!passwordResult.IsSuccess)
            errors.AddRange(passwordResult.Errors);

        // Retorna erros de validação, caso existam.
        return errors.Any()
            ? Result<User>.Error(new ErrorList(errors.ToArray()))
            : Result<User>.Success(new User(name, gender, emailResult.Value, passwordResult.Value));
    }

    /// <summary>
    /// Cria um usuário sem realizar validações internas nos parâmetros.
    /// Este método assume que <paramref name="email"/> e <paramref name="password"/> já foram validados externamente.
    /// </summary>
    /// <param name="name">Nome completo do usuário.</param>
    /// <param name="gender">Gênero do usuário.</param>
    /// <param name="email">Objeto <see cref="Email"/> já validado.</param>
    /// <param name="password">Objeto <see cref="Password"/> já validado.</param>
    /// <param name="createdAt">Data e hora de criação do usuário.</param>
    /// <returns>Um objeto <see cref="User"/> instanciado com os valores fornecidos.</returns>
    public static User Create(string name, EGender gender, Email email, Password password)
        => new(name, gender, email, password);
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe UserFactory → Implementa o padrão Factory para criação de usuários.
✅ Método Create() com validação → Usa Ardalis.Result para encapsular erros e garantir que e-mail e senha sejam válidos antes de criar um usuário.
✅ Método Create() sem validação → Assume que `Email` e `Password` já foram validados externamente e instancia o usuário diretamente.
✅ Uso de Email.Create() e Password.Create() → Aplica regras de validação antes da criação de um novo usuário.
✅ Uso de Ardalis.Result → Encapsula o resultado da operação e permite tratamento estruturado de erros.
✅ Arquitetura baseada em Domain-Driven Design → Mantém separação entre entidades e lógica de criação de objetos.
✅ Melhorias na documentação:
   - Adição de `<see cref="NomeDaClasse"/>` para referências diretas em XML Docs.
   - Explicação clara sobre os métodos e suas responsabilidades.
   - Melhor detalhamento dos parâmetros e retorno dos métodos.
✅ Essa abordagem melhora a integridade dos dados e facilita a manutenção e os testes do sistema.
*/
