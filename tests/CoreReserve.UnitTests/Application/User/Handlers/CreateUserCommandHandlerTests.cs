using Ardalis.Result;
using Bogus;
using CoreReserve.Application.User.Commands;
using CoreReserve.Application.User.Handlers;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.Factories;
using CoreReserve.Infrastructure.Data;
using CoreReserve.Infrastructure.Data.Repositories;
using CoreReserve.UnitTests.Fixtures;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Categories;

namespace CoreReserve.UnitTests.Application.User.Handlers
{
    /// <summary>
    /// Testes unitários para a classe CreateUserCommandHandler.
    /// Verifica os cenários de criação de usuário: sucesso, email duplicado e validação inválida.
    /// </summary>
    [UnitTest]
    public class CreateUserCommandHandlerTests(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
    {
        // Validador reutilizado em todos os testes
        private readonly CreateUserCommandValidator _validator = new();

        /// <summary>
        /// Testa se um comando válido para criação de usuário retorna resultado de sucesso.
        /// </summary>
        [Fact]
        public async Task Add_ValidCommand_ShouldReturnsCreatedResult()
        {
            //Arrange
            var command = new Faker<CreateUserCommand>()
            .RuleFor(command => command.Name, faker => faker.Internet.UserName())
            .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())
            .RuleFor(command => command.Email, faker => faker.Person.Email.ToLowerInvariant())
            .RuleFor(command => command.Password, faker => faker.PickRandom("Corret123@"))
            .Generate();

            var unitOfWork = new UnitOfWork(
                fixture.Context,
                Substitute.For<IEventStoreRepository>(),
                Substitute.For<IMediator>(),
                Substitute.For<ILogger<UnitOfWork>>());

            var handler = new CreateUserCommandHandler(
                _validator,
                new UserWriteOnlyRepository(fixture.Context),
                unitOfWork);

            // Act
            var act = await handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().NotBeNull();
            act.IsCreated().Should().BeTrue();
            act.Value.Should().NotBeNull();
            act.Value.Id.Should().NotBe(Guid.Empty);
        }

        /// <summary>
        /// Testa se um comando com email duplicado retorna resultado de falha.
        /// </summary>
        [Fact]
        public async Task Add_DuplicateEmailCommand_ShouldReturnsFailResult()
        {
            //Arrange
            var command = new Faker<CreateUserCommand>()
                 .RuleFor(command => command.Name, faker => faker.Internet.UserName())
            .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())
            .RuleFor(command => command.Email, faker => faker.Person.Email.ToLowerInvariant())
            .RuleFor(command => command.Password, faker => faker.PickRandom("Corret123@"))
            .Generate();

            var repository = new UserWriteOnlyRepository(fixture.Context);
            repository.Add(UserFactory.Create(
                command.Name,
                command.Gender,
                command.Email,
                command.Password
            ));

            await fixture.Context.SaveChangesAsync();
            fixture.Context.ChangeTracker.Clear();

            var handler = new CreateUserCommandHandler(
                _validator,
                repository,
                Substitute.For<IUnitOfWork>());

            //Act
            var act = await handler.Handle(command, CancellationToken.None);

            //Assert
            act.Should().NotBeNull();
            act.IsSuccess.Should().BeFalse();
            act.Errors.Should()
                .NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems()
                .And.Contain(errorMessage => errorMessage == "The provided email address is already in use.");
        }

        /// <summary>
        /// Testa se um comando inválido retorna resultado de falha com erros de validação.
        /// </summary>
        [Fact]
        public async Task Add_InvalidCommand_ShouldReturnsFailResult()
        {
            //Arrange
            var handler = new CreateUserCommandHandler(
                _validator,
                Substitute.For<IUserWriteOnlyRepository>(),
                Substitute.For<IUnitOfWork>());

            //Act
            var act = await handler.Handle(new CreateUserCommand(), CancellationToken.None);

            //Assert
            act.Should().NotBeNull();
            act.IsSuccess.Should().BeFalse();
            act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
        }
    }
}

/*
🧪 TESTES DE UNIDADE - CreateUserCommandHandler

📋 PROPÓSITO:
Testes unitários que validam o comportamento do handler de criação de usuários,
cobrindo cenários de sucesso, falha por duplicação e falha por validação.

🔧 COMPONENTES TESTADOS:
• CreateUserCommandHandler: Handler principal da operação
• CreateUserCommandValidator: Validador FluentValidation
• UserWriteOnlyRepository: Repository de escrita de usuários
• UnitOfWork: Padrão Unit of Work para transações

⚡ CENÁRIOS DE TESTE:

1️⃣ ADD_VALID_COMMAND:
   - Gera comando válido com Bogus/Faker
   - Verifica se retorna IsCreated = true
   - Valida se o ID do usuário foi gerado

2️⃣ ADD_DUPLICATE_EMAIL:
   - Cria usuário inicial no banco
   - Tenta criar outro com mesmo email
   - Verifica erro específico de email duplicado

3️⃣ ADD_INVALID_COMMAND:
   - Envia comando vazio/inválido
   - Verifica falha na validação
   - Confirma presença de erros de validação

🚀 FERRAMENTAS UTILIZADAS:
- xUnit: Framework de testes
- FluentAssertions: Assertions expressivas
- Bogus: Geração de dados fake
- NSubstitute: Mocking framework
- EfSqliteFixture: Banco em memória para testes

💡 PADRÕES APLICADOS:
- AAA (Arrange, Act, Assert)
- Isolamento de testes com fixture
- Mocking de dependências externas
- Geração de dados de teste realistas
*/