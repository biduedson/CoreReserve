using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AutoMapper;
using CoreReserve.Query.Abstractions;
using CoreReserve.Query.Data.Context;
using CoreReserve.Query.Data.Mappings;
using CoreReserve.Query.Data.Repositories;
using CoreReserve.Query.Data.Repositories.Abstractions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace CoreReserve.Query
{
    /// <summary>
    /// Classe estática para configurar os serviços da camada de consultas.
    /// Define injeção de dependência para query handlers, banco de dados NoSQL e repositórios.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ConfigureServices
    {
        /// <summary>
        /// Registra manipuladores de consultas na coleção de serviços.
        /// </summary>
        /// <param name="services">Coleção de serviços.</param>
        public static IServiceCollection AddQueryHandlers(this IServiceCollection services)
        {
            var assembly = Assembly.GetAssembly(typeof(IQueryMarker));
            return services
                .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly!)) // Registra handlers do MediatR.
                .AddSingleton<IMapper>(new Mapper(new MapperConfiguration(cfg => cfg.AddMaps(assembly)))) // Configura AutoMapper.
                .AddValidatorsFromAssembly(assembly); // Adiciona validadores do FluentValidation.
        }

        /// <summary>
        /// Registra o contexto de leitura do banco de dados NoSQL na coleção de serviços.
        /// </summary>
        /// <param name="services">Coleção de serviços.</param>
        public static IServiceCollection AddReadDbContext(this IServiceCollection services)
        {
            services
                .AddScoped<ISynchronizeDb, NoSqlDbContext>() // Registra o serviço de sincronização.
                .AddScoped<IReadDbContext, NoSqlDbContext>() // Registra o contexto de leitura.
                .AddScoped<NoSqlDbContext>(); // Adiciona o contexto do banco NoSQL.

            ConfigureMongoDb(); // Configura MongoDB.

            return services;
        }

        /// <summary>
        /// Registra repositórios somente leitura na coleção de serviços.
        /// </summary>
        /// <param name="services">Coleção de serviços.</param>
        public static IServiceCollection AddReadOnlyRepositories(this IServiceCollection services) =>
            services.AddScoped<IUserReadOnlyRepository, UserReadOnlyRepository>();

        /// <summary>
        /// Configura os parâmetros e convenções do MongoDB.
        /// </summary>
        private static void ConfigureMongoDb()
        {
            try
            {
                // Passo 1: Configura o serializador para tipo Guid.
                BsonSerializer.TryRegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

                // Passo 2: Configurações de convenção para todos os mapeamentos.
                ConventionRegistry.Register("Conventions",
                    new ConventionPack
                    {
                    new CamelCaseElementNameConvention(), // Converte nomes de elementos para camel case.
                    new EnumRepresentationConvention(BsonType.String), // Serializa enums como strings.
                    new IgnoreExtraElementsConvention(true), // Ignora elementos extras ao desserializar.
                    new IgnoreIfNullConvention(true) // Ignora valores nulos ao serializar.
                    }, _ => true);

                // Passo 3: Registra configurações de mapeamento.
                new UserMap().Configure(); // Configuração da classe Customer.
            }
            catch
            {
                // Ignora exceções durante a configuração.
            }
        }
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe ConfigureServices → Responsável pela configuração dos serviços da camada de consulta.
✅ Implementação de métodos de extensão → Permite adicionar serviços diretamente à IServiceCollection.
✅ Uso de MediatR → Facilita a comunicação entre componentes desacoplados com handlers de consultas.
✅ Uso de AutoMapper → Automatiza a conversão entre objetos de domínio e modelos de consulta.
✅ Uso de FluentValidation → Adiciona validação para consultas antes de serem processadas.
✅ Uso de MongoDB → Configura a conexão com banco NoSQL, incluindo serialização e convenções.
✅ Método ConfigureMongoDb() → Define serializadores, convenções e mapeamentos para garantir compatibilidade.
✅ Arquitetura baseada em CQRS → Separa operações de leitura e escrita, garantindo escalabilidade e organização.
✅ Essa abordagem melhora a manutenção do código e torna o sistema modular e eficiente.
*/
