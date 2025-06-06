using CoreReserve.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CoreReserve.PublicApi.Factories;

/// <summary>
/// Fábrica para criação do contexto do banco de dados `EventStoreDbContext`.
/// Usado especificamente para **design-time**, como geração de migrações pelo Entity Framework.
/// </summary>
public class EventStoreDbContextFactory : IDesignTimeDbContextFactory<EventStoreDbContext>
{
    /// <summary>
    /// Cria uma instância do contexto `EventStoreDbContext` baseada na configuração do ambiente.
    /// Esse método é usado para **geração de migrações** via `dotnet ef migrations` e não é chamado diretamente na aplicação.
    /// </summary>
    /// <param name="args">Parâmetros opcionais usados pelo `IDesignTimeDbContextFactory`.</param>
    /// <returns>Uma instância configurada de <see cref="EventStoreDbContext"/>.</returns>
    public EventStoreDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        // 🔹 Carrega as configurações do arquivo `appsettings.json`, variáveis de ambiente e `secrets.json`.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<EventStoreDbContextFactory>() // 🔹 Adiciona suporte para secrets.json
            .AddEnvironmentVariables()
            .Build();

        // 🔹 Obtém a string de conexão do banco de dados a partir da configuração.
        var sqlConnection = configuration.GetSection("ConnectionStrings")["SqlConnection"];

        // 🔹 Garante que a string de conexão está definida antes de continuar.
        if (string.IsNullOrEmpty(sqlConnection))
        {
            throw new InvalidOperationException("SqlConnection was not found in configuration.");
        }

        // 🔹 Configura o contexto do banco de dados com a string de conexão definida.
        var optionsBuilder = new DbContextOptionsBuilder<EventStoreDbContext>();
        optionsBuilder.UseSqlServer(sqlConnection, b => b.MigrationsAssembly("CoreReserve.PublicApi"));

        return new EventStoreDbContext(optionsBuilder.Options);
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe EventStoreDbContextFactory → Implementa `IDesignTimeDbContextFactory` para suporte a migrações do EF Core.
✅ Método CreateDbContext() → Instancia o `EventStoreDbContext` com base na configuração do ambiente.
✅ Uso de ConfigurationBuilder → Carrega configurações de `appsettings.json`, `secrets.json` e variáveis de ambiente.
✅ Uso de `secrets.json` → Protege dados sensíveis, como strings de conexão do banco.
✅ Validação da string de conexão → Garante que `SqlConnection` esteja definida antes de instanciar o contexto.
✅ Configuração do banco de dados → Define SQL Server como provedor e especifica o assembly de migrações.
✅ Arquitetura baseada em boas práticas → Mantém separação entre **configuração** e **execução**, facilitando manutenção.
✅ Essa abordagem melhora a segurança e facilita a execução de migrações do banco de dados.
*/