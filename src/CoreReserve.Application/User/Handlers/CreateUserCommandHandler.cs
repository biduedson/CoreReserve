using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CoreReserve.Application.User.Commands;
using CoreReserve.Application.User.Responses;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.Factories;
using CoreReserve.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace CoreReserve.Application.User.Handlers
{
    /// <summary>
    /// Manipulador do comando <see cref="CreateUserCommand"/>.
    /// Responsável por validar, criar e persistir um novo cliente.
    /// </summary>
    public class CreateUserCommandHandler(
    IValidator<CreateUserCommand> validator,
    IUserWriteOnlyRepository repository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateUserCommand, Result<CreatedUserResponse>>
    {
        /// <summary>
        /// Processa a criação de um novo cliente com base no comando recebido.
        /// </summary>
        /// <param name="request">O comando contendo os dados do cliente.</param>
        /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
        /// <returns>Resultado da operação com o ID do cliente criado.</returns>
        public async Task<Result<CreatedUserResponse>> Handle(
            CreateUserCommand request,
            CancellationToken cancellationToken)
        {
            // Valida a requisição.
            var validationResult = await validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
            {
                // Retorna resultado com erros de validação.
                return Result<CreatedUserResponse>.Invalid(validationResult.AsErrors());
            }

            // Instancia o objeto de valor Email.
            var email = Email.Create(request.Email).Value;

            // Verifica se já existe um cliente com o mesmo e-mail.
            if (await repository.ExistsByEmailAsync(email))
                return Result<CreatedUserResponse>.Error("The email address you provided is already in use.");

            // Cria uma instância da entidade Cliente.
            // Ao ser criada, o evento "UserCreatedEvent" será gerado automaticamente.
            var user = UserFactory.Create(
                request.Name,
                request.Gender,
                email.Address,
                request.Password
            );

            // Adiciona a entidade ao repositório.
            repository.Add(user);

            // Salva as mudanças no banco de dados e dispara eventos relacionados.
            await unitOfWork.SaveChangesAsync();

            // Retorna o ID do cliente criado.
            return Result<CreatedUserResponse>.Created(
                new CreatedUserResponse(user.Value.Id), location: $"/api/users/{user.Value.Id}");
        }
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
// -----------------------------------------
/*
✅ Classe CreateCUserCommandHandler → Processa o comando para criação de um novo cliente. 
✅ Uso de FluentValidation → Garante que os dados da requisição sejam válidos antes de continuar a execução. 
✅ Implementação de IUnitOfWork → Gerencia transações garantindo que as mudanças sejam persistidas corretamente. 
✅ Verificação de e-mail duplicado → Impede que clientes sejam registrados com e-mails já existentes. 
✅ Geração automática do evento "CustomerUserEvent" → Permite rastrear operações realizadas sobre clientes. 
✅ Essa estrutura promove modularidade, consistência e segurança na criação de clientes dentro do sistema. 
*/