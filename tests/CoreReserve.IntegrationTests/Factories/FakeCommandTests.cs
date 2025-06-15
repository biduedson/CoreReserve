using Bogus;
using CoreReserve.Application.User.Commands;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.ValueObjects;
using CoreReserve.Query.QueriesModel;


namespace CoreReserve.IntegrationTests.Factories
{
    /// <summary>
    /// Factory para geração de dados fictícios utilizados em testes de integração
    /// Utiliza a biblioteca Bogus para criar objetos com dados realistas
    /// </summary>
    internal static class FakeCommandTests
    {
        #region CreateUserCommand Factory Methods

        /// <summary>
        /// Gera um comando de criação de usuário com dados válidos e realistas
        /// </summary>
        /// <returns>CreateUserCommand com dados fictícios válidos</returns>
        public static CreateUserCommand FakeCreateUserCommand()
        {
            return new Faker<CreateUserCommand>()
                // Gera nome de usuário fictício baseado em pessoa real
                .RuleFor(command => command.Name, faker => faker.Person.UserName)

                // Gera email fictício válido
                .RuleFor(command => command.Email, faker => faker.Person.Email)

                // Escolhe gênero aleatório do enum disponível
                .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())

                // Define password fixo válido para testes (atende critérios de segurança)
                .RuleFor(command => command.Password, faker => faker.PickRandom("Corret123@"))
                .Generate();
        }

        /// <summary>
        /// Gera um comando de criação de usuário com dados inválidos para testes negativos
        /// </summary>
        /// <returns>CreateUserCommand com campos nulos/vazios</returns>
        public static CreateUserCommand InvalidCreateUserCommand()
        {
            // Gera comando sem configurar regras - resulta em campos nulos/vazios
            return new Faker<CreateUserCommand>().Generate();
        }

        /// <summary>
        /// Gera comando de criação de usuário com email específico fornecido
        /// </summary>
        /// <param name="email">Email específico a ser usado no comando</param>
        /// <returns>CreateUserCommand com email customizado</returns>
        public static CreateUserCommand CreateUserCommandWhitEmail(string email)
        {
            return new Faker<CreateUserCommand>()
                // Gera nome de usuário fictício
                .RuleFor(command => command.Name, faker => faker.Person.UserName)

                // Usa o email fornecido como parâmetro
                .RuleFor(command => command.Email, faker => email)

                // Escolhe gênero aleatório
                .RuleFor(command => command.Gender, faker => faker.PickRandom<EGender>())

                // Define password fixo válido
                .RuleFor(command => command.Password, faker => faker.PickRandom("Corret123@"))
                .Generate();
        }

        #endregion

        #region User Entity Factory Methods

        /// <summary>
        /// Cria entidade User do domínio com dados fictícios válidos
        /// Utiliza Value Objects para garantir consistência dos dados
        /// </summary>
        /// <returns>Entidade User com dados fictícios</returns>
        public static User CreateUserFake()
        {
            return new Faker<User>()
                .CustomInstantiator(faker =>
                {
                    // Cria Value Object Email com validação
                    var emailResult = Email.Create(faker.Internet.Email());

                    // Cria Value Object Password com critérios de segurança
                    // Garante: mínimo 6 caracteres + maiúscula + número + caractere especial
                    var passwordResult = Password.Create($"{faker.Internet.Password(6, false)}A1!");

                    // Instancia User com construtor do domínio
                    return new User(
                        faker.Person.UserName,              // Nome do usuário
                        faker.PickRandom<EGender>(),         // Gênero aleatório
                        emailResult.Value,                   // Email validado
                        passwordResult.Value                 // Password validado
                    );
                }).Generate();
        }

        #endregion

        #region UserQueryModel Factory Methods

        /// <summary>
        /// Cria modelo de consulta de usuário único com dados fictícios
        /// Usado para simular retornos de queries de leitura
        /// </summary>
        /// <returns>UserQueryModel com dados fictícios</returns>
        public static UserQueryModel CreateUserQueryModelFake()
        {
            return new Faker<UserQueryModel>()
                .CustomInstantiator(faker =>
                {
                    // Cria email válido para extração do endereço
                    var emailResult = Email.Create(faker.Internet.Email());

                    // Instancia modelo de query com dados realistas
                    return new UserQueryModel(
                        faker.Random.Guid(),                    // ID único
                        faker.Internet.UserName(),              // Nome de usuário
                        faker.PickRandom<EGender>().ToString(),  // Gênero como string
                        emailResult.Value.Address,              // Endereço de email extraído
                        faker.Date.Past(30).Date                // Data de criação nos últimos 30 dias
                    );
                }).Generate();
        }

        /// <summary>
        /// Cria coleção de modelos de consulta de usuários com quantidade específica
        /// Útil para testes de paginação e listagem
        /// </summary>
        /// <param name="quantity">Quantidade de usuários a serem gerados</param>
        /// <returns>Coleção de UserQueryModel com dados fictícios</returns>
        public static IEnumerable<UserQueryModel> CreateUsersQueryModelFake(int quantity)
        {
            return new Faker<UserQueryModel>()
                .CustomInstantiator(faker =>
                {
                    // Cria email válido para cada usuário da coleção
                    var emailResult = Email.Create(faker.Internet.Email());

                    // Instancia modelo com dados únicos para cada iteração
                    return new UserQueryModel(
                        faker.Random.Guid(),                    // ID único para cada usuário
                        faker.Internet.UserName(),              // Nome único
                        faker.PickRandom<EGender>().ToString(),  // Gênero aleatório
                        emailResult.Value.Address,              // Email único
                        faker.Date.Past(30).Date                // Data aleatória nos últimos 30 dias
                    );
                }).Generate(quantity); // Gera a quantidade especificada
        }

        #endregion
    }
}

/*
 * EXPLICAÇÃO GERAL DA CLASSE FakeCommandTests:
 * 
 * Esta classe é uma factory estática responsável por gerar dados fictícios 
 * para testes de integração no projeto CoreReserve, utilizando a biblioteca Bogus.
 * 
 * PRINCIPAIS FUNCIONALIDADES:
 * 
 * 1. GERAÇÃO DE DADOS REALISTAS:
 *    - Utiliza Bogus para criar dados que simulam cenários reais
 *    - Gera nomes, emails, datas e outros dados com padrões consistentes
 *    - Garante que os dados gerados atendam às regras de negócio
 * 
 * 2. TIPOS DE OBJETOS SUPORTADOS:
 *    - CreateUserCommand: Comandos de aplicação para criar usuários
 *    - User: Entidades de domínio com Value Objects
 *    - UserQueryModel: Modelos de consulta para operações de leitura
 * 
 * 3. CENÁRIOS DE TESTE COBERTOS:
 *    - Dados válidos: Para testes de fluxo normal (happy path)
 *    - Dados inválidos: Para testes de validação e tratamento de erros
 *    - Dados customizados: Para cenários específicos (ex: email específico)
 *    - Coleções: Para testes de paginação e operações em lote
 * 
 * 4. PADRÕES IMPLEMENTADOS:
 *    - Factory Pattern: Centraliza criação de objetos de teste
 *    - Builder Pattern: Usa Faker para construir objetos complexos
 *    - Value Object Pattern: Respeita a criação de Value Objects do domínio
 * 
 * 5. VALIDAÇÕES E CONSISTÊNCIA:
 *    - Emails sempre válidos através do Value Object Email
 *    - Passwords que atendem critérios de segurança
 *    - Gêneros válidos conforme enum do domínio
 *    - Datas realistas para simulação de dados históricos
 * 
 * VANTAGENS DESTA ABORDAGEM:
 * - Centralização da geração de dados de teste
 * - Consistência entre diferentes testes
 * - Facilidade de manutenção (mudanças em um local)
 * - Dados realistas que revelam problemas reais
 * - Suporte a diferentes cenários de teste
 * - Reutilização de código entre testes
 * - Redução de boilerplate nos testes
 * 
 * USO TÍPICO EM TESTES:
 * - Arrange: var command = FakeCommandTests.FakeCreateUserCommand();
 * - Act: var result = await handler.Handle(command);
 * - Assert: result.Should().BeSuccessful();
 * 
 * OBSERVAÇÕES IMPORTANTES:
 * - Métodos estáticos para facilitar acesso
 * - Dados sempre novos a cada chamada (não cached)
 * - Compatível com bibliotecas de assertion como FluentAssertions
 * - Suporta cenários tanto de sucesso quanto de falha
 */