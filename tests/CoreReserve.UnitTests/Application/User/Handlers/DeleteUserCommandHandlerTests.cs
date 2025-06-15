using Bogus;
using CoreReserve.Application.User.Commands;
using CoreReserve.Application.User.Handlers;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.Factories;
using CoreReserve.Infrastructure.Data;
using CoreReserve.Infrastructure.Data.Repositories;
using CoreReserve.Query.Data.Repositories.Abstractions;
using CoreReserve.UnitTests.Fixtures;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Categories;

namespace CoreReserve.UnitTests.Application.User.Handlers
{
    /// <summary>
    /// Testes unitários para a classe DeleteUserCommandHandler.
    /// Verifica os cenários de exclusão de usuário: sucesso, usuário não encontrado e validação inválida.
    /// </summary>
    [UnitTest]
    public class DeleteUserCommandHandlerTests(EfSqliteFixture fixture) : IClassFixture<EfSqliteFixture>
    {
        /// <summary>
        /// Validador reutilizado em todos os testes para validação de comandos de exclusão.
        /// </summary>
        private readonly DeleteUserCommandValidator _validator = new();

        /// <summary>
        /// Testa se um comando válido para exclusão de usuário existente retorna resultado de sucesso.
        /// </summary>
        [Fact]
        public async Task Delete_ValidUserId_ShouldReturnsSuccessResult()
        {
            //Arrange
            var user = new Faker<CoreReserve.Domain.Entities.UserAggregate.User>()
                 .CustomInstantiator(faker => UserFactory.Create(
                  faker.Internet.UserName(),
                  faker.PickRandom<EGender>(),
                  faker.Person.Email,
                  faker.PickRandom("Corret123@")))
                .Generate();

            var repository = new UserWriteOnlyRepository(fixture.Context);
            repository.Add(user);

            await fixture.Context.SaveChangesAsync();
            fixture.Context.ChangeTracker.Clear();

            var unitOfWork = new UnitOfWork(
                fixture.Context,
                Substitute.For<IEventStoreRepository>(),
                Substitute.For<IMediator>(),
                Substitute.For<ILogger<UnitOfWork>>());

            var handler = new DeleteUserCommandHandler(
                _validator,
                new UserWriteOnlyRepository(fixture.Context),
                unitOfWork);

            var command = new DeleteUserCommand(user.Id);

            //Act
            var act = await handler.Handle(command, CancellationToken.None);

            // Assert
            act.Should().NotBeNull();
            act.IsSuccess.Should().BeTrue();
            act.SuccessMessage.Should().Be("Removed successfully!");
        }

        /// <summary>
        /// Testa se um comando com ID de usuário inexistente retorna resultado de falha.
        /// </summary>
        [Fact]
        public async Task Delete_NotFoundUser_ShouldReturnsFailResult()
        {
            //Arrange
            var command = new DeleteUserCommand(Guid.NewGuid());

            var handler = new DeleteUserCommandHandler(
                _validator,
                new UserWriteOnlyRepository(fixture.Context),
                Substitute.For<IUnitOfWork>());

            //Act
            var act = await handler.Handle(command, CancellationToken.None);

            //Assert
            act.Should().NotBeNull();
            act.IsSuccess.Should().BeFalse();
            act.Errors.Should()
                .NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems()
                .And.Contain(errorMessage => errorMessage == $"No user found with Id: {command.Id}");
        }

        /// <summary>
        /// Testa se um comando inválido (ID vazio) retorna resultado de falha com erros de validação.
        /// </summary>
        [Fact]
        public async Task Delete_InvalidCommand_ShouldReturnsFailResult()
        {
            //Arrange
            var handler = new DeleteUserCommandHandler(
                _validator,
                Substitute.For<IUserWriteOnlyRepository>(),
                Substitute.For<IUnitOfWork>());

            //Act
            var act = await handler.Handle(new DeleteUserCommand(Guid.Empty), CancellationToken.None);

            //Assert
            act.Should().NotBeNull();
            act.IsSuccess.Should().BeFalse();
            act.ValidationErrors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
        }
    }
}

/*
🧪 TESTES DE UNIDADE - DeleteUserCommandHandler

📋 PROPÓSITO:
Conjunto de testes unitários que valida o comportamento do handler de exclusão de usuários,
garantindo que todos os cenários possíveis sejam cobertos adequadamente.

🔧 COMPONENTES TESTADOS:
• DeleteUserCommandHandler: Handler responsável pela exclusão
• DeleteUserCommandValidator: Validação de entrada do comando
• UserWriteOnlyRepository: Repository para operações de escrita
• UnitOfWork: Controle de transações e persistência

⚡ CENÁRIOS COBERTOS:

1️⃣ DELETE_VALID_USER_ID:
   - Cria usuário usando UserFactory com dados gerados pelo Bogus
   - Persiste no banco em memória via EfSqliteFixture
   - Executa exclusão e verifica mensagem de sucesso

2️⃣ DELETE_NOT_FOUND_USER:
   - Usa ID inexistente (Guid.NewGuid())
   - Verifica retorno de erro específico "No user found with Id"
   - Confirma que operação falha adequadamente

3️⃣ DELETE_INVALID_COMMAND:
   - Envia comando com Guid.Empty (inválido)
   - Testa validação de entrada via FluentValidation
   - Verifica presença de erros de validação

🚀 FERRAMENTAS E PADRÕES:
- Bogus/Faker: Geração de dados realistas de teste
- FluentAssertions: Assertions expressivas e legíveis
- NSubstitute: Mocking para isolamento de dependências
- EfSqliteFixture: Banco em memória para testes rápidos
- AAA Pattern: Arrange-Act-Assert para estrutura clara
*/