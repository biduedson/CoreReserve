
using Microsoft.Extensions.DependencyInjection;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Infrastructure.Data.Context;
using CoreReserve.IntegrationTests.Extensions;
using Xunit.Categories;
using Xunit;
using CoreReserve.IntegrationTests.Abstractions;
using CoreReserve.IntegrationTests.Factories;
using CoreReserve.Query.Data.Repositories.Abstractions;
using NSubstitute;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CoreReserve.Core.Extensions;
using CoreReserve.PublicApi.Models;
using CoreReserve.Query.QueriesModel;
using FluentAssertions;
using System.Net;
using Microsoft.AspNetCore.Http;
using CoreReserve.Application.User.Responses;



namespace CoreReserve.IntegrationTests.Controllers.V1
{

    [IntegrationTest]
    public class UsersControllerTests : TestBase
    {
        #region Constants & Configuration

        private const string USERS_ENDPOINT = "/api/users";

        #endregion

        #region POST /api/users  - Testes para criação de usuarios

        /// <summary>
        /// Cenário: Criação de usuário com dados válidos
        /// 
        /// Dado que eu tenho dados válidos de usuário
        /// Quando eu faço uma requisição POST para criar o usuário
        /// Então deve retornar 201 Created
        /// E deve incluir o ID do usuário criado na resposta
        /// E deve definir o header Location apropriado
        /// </summary>
        [Fact]
        public async Task CreateUser_WithValidData_ShouldReturn201CreatedWithUserDetails()
        {
            // Arrange - Preparação do teste
            // Inicializa a factory da aplicação web para simular o ambiente real
            await using var webApplicationFactory = InitializeWebAppFactory();
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            var command = FakeCommandTests.FakeCreateUserCommand();

            // Act - Execução da ação a ser testada
            // Converte o comando para JSON e envia via POST para a API
            using var jsonContent = command.ToJsonHttpContent();
            using var act = await httpClient.PostAsync(USERS_ENDPOINT, jsonContent);

            // Assert - Verificações do resultado
            // Verifica se a resposta HTTP está correta
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeTrue();
            act.StatusCode.Should().Be(HttpStatusCode.Created); // Status 201 para criação bem-sucedida
                                                                // Verifica se o conteúdo da resposta está correto
            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreatedUserResponse>>();
            response.Should().NotBeNull();
            response.Success.Should().BeTrue(); // Campo success deve ser true
            response.StatusCode.Should().Be(StatusCodes.Status201Created);
            response.Errors.Should().BeEmpty(); // Não deve haver erros
            response.Result.Should().NotBeNull();
            response.Result.Id.Should().NotBeEmpty(); // ID do cliente criado deve ser gerado

            // Verifica se o header Location foi definido corretamente (padrão REST)
            act.Headers.GetValues("Location").Should().NotBeNullOrEmpty()
                .And.Contain($"/api/users/{response.Result.Id}");
        }

        /// <summary>
        /// Cenário: Tentativa de criação com dados inválidos
        /// 
        /// Dado que eu tenho dados inválidos de usuário (campos obrigatórios em branco)
        /// Quando eu faço uma requisição POST
        /// Então deve retornar 400 Bad Request
        /// E deve incluir detalhes dos erros de validação
        /// </summary>
        [Fact]
        public async Task CreateUser_WithInvalidData_ShouldReturn400BadRequestWithValidationErrors()
        {
            // Arrange - Preparação
            await using var webApplicationFactory = InitializeWebAppFactory();
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            // Gera um comando com dados inválidos (sem especificar regras, os campos ficam nulos/vazios)
            var command = FakeCommandTests.InvalidCreateUserCommand();

            // Act - Envia requisição com dados inválidos
            using var jsonContent = command.ToJsonHttpContent();
            using var act = await httpClient.PostAsync(USERS_ENDPOINT, jsonContent);

            // Assert - Verifica se retorna erro de validação
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeFalse();
            act.StatusCode.Should().Be(HttpStatusCode.BadRequest); // Status 400 para dados inválidos

            // Verifica se a resposta contém os erros de validação
            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreatedUserResponse>>();
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            response.Result.Should().BeNull(); // Não deve retornar resultado quando há erro
            response.Errors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems(); // Deve conter erros únicos
        }

        /// <summary>
        /// Cenário: Tentativa de criação com email duplicado
        /// Dado que já existe um usuário com um email específico
        /// Quando eu tento criar outro usuário com o mesmo email
        /// Então deve retornar 400 Bad Request
        /// E deve incluir erro específico sobre email duplicado
        /// </summary>
        [Fact]
        public async Task CreateUser_WithDuplicateEmail_ShouldReturn400BadRequestWithSpecificError()
        {
            // Arrange - Cria um cliente existente no banco primeiro
            var user = FakeCommandTests.CreateUserFake();

            // Inicializa a aplicação e adiciona o cliente existente no banco
            await using var webApplicationFactory = InitializeWebAppFactory(configureServiceScope: serviceScope =>
             {
                 var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
                 writeDbContext.Users.Add(user);
                 writeDbContext.SaveChanges(); // Persiste o cliente no banco
             });
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            // Cria um comando com o mesmo email do cliente existente
            var command = FakeCommandTests.CreateUserCommandWhitEmail(user.Email.Address);


            // Act - Tenta criar cliente com email duplicado
            using var jsonContent = command.ToJsonHttpContent();
            using var act = await httpClient.PostAsync(USERS_ENDPOINT, jsonContent);

            // Assert - Verifica se retorna erro específico de email duplicado
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeFalse();
            act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<CreatedUserResponse>>();
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            response.Result.Should().BeNull();
            response.Errors.Should().NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems()
                .And.AllSatisfy(error => error.Message.Should().Be("The provided email address is already in use."));
        }

        #endregion

        #region GET: /api/users/ - Testes para listar todos os usuarios
        /// <summary>
        /// Testa se a API retorna status 200 OK ao buscar todos os usuarios
        /// Este teste verifica se a consulta de todos os usuarios funciona corretamente
        /// </summary>
        [Fact]
        public async Task Should_ReturnsHttpStatus200Ok_When_GetAll()
        {
            // Arrange - Cria dados mock para o repository de leitura
            var queryModels = FakeCommandTests.CreateUsersQueryModelFake(10);// Gera 10 clientes fictícios

            // Cria um mock do repository usando NSubstitute
            var readOnlyRepository = Substitute.For<IUserReadOnlyRepository>();
            readOnlyRepository.GetAllAsync().Returns(queryModels); // Configura o retorno do mock

            // Inicializa a aplicação substituindo o repository real pelo mock
            await using var webApplicationFactory = InitializeWebAppFactory(services =>
            {
                services.RemoveAll<IUserReadOnlyRepository>(); // Remove implementação real
                services.AddScoped(_ => readOnlyRepository); // Adiciona o mock
            });
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            // Act - Faz requisição GET para listar todos os clientes
            using var act = await httpClient.GetAsync(USERS_ENDPOINT);


            // Assert - Verifica se a resposta está correta
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeTrue();
            act.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verifica o conteúdo da resposta
            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<IEnumerable<UserQueryModel>>>();
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Errors.Should().BeEmpty();
            response.Result.Should().NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems()
                .And.HaveCount(queryModels.Count()) // Deve retornar a quantidade correta
                .And.AllSatisfy(model => // Verifica se todos os modelos têm dados válidos
                {
                    model.Id.Should().NotBeEmpty();
                    model.Name.Should().NotBeNullOrWhiteSpace();
                    model.Email.Should().NotBeNullOrWhiteSpace();
                    model.Gender.Should().NotBeNullOrWhiteSpace();
                });

            // Verifica se o repository foi chamado uma vez
            await readOnlyRepository.Received(1).GetAllAsync();
        }

        #endregion

        #region GET: /api/users/{id} - Testes para buscar usuario por ID
        /// <summary>
        /// Testa se a API retorna status 200 OK ao buscar um usuario específico por ID válido
        /// </summary>
        [Fact]
        public async Task Should_ReturnsHttpStatus200Ok_When_GetById_ValidRequest()
        {
            // Arrange - Cria um cliente mock
            var queryModel = FakeCommandTests.CreateUserQueryModelFake();

            // Configura o mock do repository para retornar o cliente quando buscado por ID
            var readOnlyRepository = Substitute.For<IUserReadOnlyRepository>();
            readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(queryModel);

            // Inicializa a aplicação substituindo o repository real pelo mock
            await using var webApplicationFactory = InitializeWebAppFactory(services =>
            {
                services.RemoveAll<IUserReadOnlyRepository>();
                services.AddScoped(_ => readOnlyRepository);
            });
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            // Act - Busca cliente por ID específico
            using var act = await httpClient.GetAsync($"{USERS_ENDPOINT}/{queryModel.Id}");


            // Assert - Verifica se retorna o cliente correto
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeTrue();
            act.StatusCode.Should().Be(HttpStatusCode.OK);

            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<UserQueryModel>>();
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Errors.Should().BeEmpty();
            response.Result.Should().NotBeNull();

            // Verifica se todos os campos do cliente retornado estão corretos
            response.Result.Id.Should().NotBeEmpty().And.Be(queryModel.Id);
            response.Result.Name.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.Name);
            response.Result.Gender.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.Gender);
            response.Result.Email.Should().NotBeNullOrWhiteSpace().And.Be(queryModel.Email);
            response.Result.CreatedAt.Should().Be(queryModel.CreatedAt);

            // Verifica se o repository foi chamado com o ID correto
            await readOnlyRepository.Received(1).GetByIdAsync(Arg.Is<Guid>(id => id == queryModel.Id));
        }


        /// <summary>
        /// Testa se a API retorna status 404 Not Found quando busca por cliente inexistente
        /// </summary>
        [Fact]
        public async Task Should_ReturnsHttpStatus400BadRequest_When_GetById_InvalidRequest()
        {
            // Arrange - Configura repository para retornar null
            var readOnlyRepository = Substitute.For<IUserReadOnlyRepository>();
            readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((UserQueryModel)null);

            // Inicializa a aplicação substituindo o repository real pelo mock
            await using var webApplicationFactory = InitializeWebAppFactory(services =>
            {
                services.RemoveAll<IUserReadOnlyRepository>();
                services.AddScoped(_ => readOnlyRepository);
            });
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            var userId = Guid.Empty; // ID inválido

            // Act - Tenta buscar com ID inválido
            using var act = await httpClient.GetAsync($"{USERS_ENDPOINT}/{userId}");

            // Assert - Verifica se retorna erro de validação
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeFalse();
            act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Verifica o conteúdo da resposta
            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<UserQueryModel>>();
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            response.Result.Should().BeNull();
            response.Errors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();

            // Verifica se o repository NÃO foi chamado devido à validação falhar antes
            await readOnlyRepository.DidNotReceive().GetByIdAsync(Arg.Is<Guid>(id => id == userId));

        }
        /// <summary>
        /// Testa se a API retorna status 404 Not Found quando busca por cliente inexistente
        /// </summary>
        [Fact]
        public async Task Should_ReturnsStatus404NotFound_When_GetById_NonExistingCustomer()
        {
            // Arrange - Configura repository para simular cliente não encontrado
            var readOnlyRepository = Substitute.For<IUserReadOnlyRepository>();
            readOnlyRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((UserQueryModel)null);

            // Inicializa a aplicação substituindo o repository real pelo mock
            await using var webApplicationFactory = InitializeWebAppFactory(services =>
            {
                services.RemoveAll<IUserReadOnlyRepository>();
                services.AddScoped(_ => readOnlyRepository);
            });
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            var userId = Guid.NewGuid(); // ID válido mas inexistente

            // Act - Busca cliente inexistente
            using var act = await httpClient.GetAsync($"{USERS_ENDPOINT}/{userId}");

            // Assert - Verifica se retorna 404
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeFalse();
            act.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Verifica o conteúdo da resposta
            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse<UserQueryModel>>();
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            response.Result.Should().BeNull();
            response.Errors.Should().NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems()
                .And.AllSatisfy(error => error.Message.Should().Be($"No user found with Id: {userId}"));

            // Verifica se o repository foi chamado (passa na validação mas não encontra)
            await readOnlyRepository.Received(1).GetByIdAsync(Arg.Is<Guid>(id => id == userId));
        }
        #endregion

        #region DELETE: /api/users/{id} - Testes para exclusão de usuarios

        /// <summary>
        /// Testa se a API retorna status 200 OK ao excluir um cliente existente
        /// </summary>
        [Fact]
        public async Task Should_ReturnsHttpStatus200Ok_When_Delete_ValidRequest()
        {
            // Arrange - Cria um usuario no banco para ser excluído
            var user = FakeCommandTests.CreateUserFake();

            // Adiciona o usuario no banco antes do teste
            await using var webApplicationFactory = InitializeWebAppFactory(configureServiceScope: serviceScope =>
            {
                var writeDbContext = serviceScope.ServiceProvider.GetRequiredService<WriteDbContext>();
                writeDbContext.Users.Add(user);
                writeDbContext.SaveChanges();
            });
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            using var act = await httpClient.DeleteAsync($"{USERS_ENDPOINT}/{user.Id}");

            // Assert - Verifica se a exclusão foi bem-sucedida
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeTrue();
            act.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verifica o conteúdo da resposta
            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse>();
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.StatusCode.Should().Be(StatusCodes.Status200OK);
            response.Errors.Should().BeEmpty();
        }

        /// <summary>
        /// Testa se a API retorna status 400 Bad Request ao tentar excluir com ID inválido
        /// </summary>
        [Fact]
        public async Task Should_ReturnsHttpStatus400BadRequest_When_Delete_InvalidRequest()
        {
            // Arrange
            await using var webApplicationFactory = InitializeWebAppFactory();
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            // Act - Tenta excluir com ID inválido (Guid.Empty)
            using var act = await httpClient.DeleteAsync($"{USERS_ENDPOINT}/{Guid.Empty}");

            // Assert - Verifica se retorna erro de validação
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeFalse();
            act.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Verifica o conteúdo da resposta
            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse>();
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            response.Errors.Should().NotBeNullOrEmpty().And.OnlyHaveUniqueItems();
        }

        /// <summary>
        /// Testa se a API retorna status 404 Not Found ao tentar excluir cliente inexistente
        /// </summary>
        [Fact]
        public async Task Should_Returistatus404NotFound_When_Delete_NonExistingUser()
        {
            // Arrange
            var userId = Guid.NewGuid(); // ID válido mas de cliente inexistente

            await using var webApplicationFactory = InitializeWebAppFactory();
            using var httpClient = webApplicationFactory.CreateClient(CreateClientOptions());

            // Act - Tenta excluir cliente inexistente
            using var act = await httpClient.DeleteAsync($"{USERS_ENDPOINT}/{userId}");

            // Assert - Verifica se retorna 404
            act.Should().NotBeNull();
            act.IsSuccessStatusCode.Should().BeFalse();
            act.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Verifica o conteúdo da resposta
            var response = (await act.Content.ReadAsStringAsync()).FromJson<ApiResponse>();
            response.Should().NotBeNull();
            response.Success.Should().BeFalse();
            response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            response.Errors.Should().NotBeNullOrEmpty()
                .And.OnlyHaveUniqueItems()
                .And.AllSatisfy(error => error.Message.Should().Be($"No user found with Id: {userId}"));
        }
        #endregion


        #region Future Test Placeholders

        // TODO: Implementar testes para outros cenários:
        // - GET /api/users/{id} - Busca por ID
        // - GET /api/users - Listagem com paginação
        // - PUT /api/users/{id} - Atualização
        // - DELETE /api/users/{id} - Exclusão
        // - Testes de autorização/autenticação
        // - Testes de performance com grande volume de dados

        #endregion
    }
}