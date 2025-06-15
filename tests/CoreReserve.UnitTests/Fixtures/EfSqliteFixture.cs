using CoreReserve.Infrastructure.Data.Context;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CoreReserve.UnitTests.Fixtures
{
    /// <summary>
    /// Fixture para testes unitários que utiliza SQLite em memória com Entity Framework.
    /// Implementa IAsyncLifetime para gerenciamento do ciclo de vida dos testes e
    /// IDisposable para liberação adequada de recursos.
    /// </summary>
    /// <remarks>
    /// Esta classe é utilizada para criar um ambiente de teste isolado com banco de dados
    /// em memória, garantindo que cada teste execute em um contexto limpo e independente.
    /// </remarks>
    public class EfSqliteFixture : IAsyncLifetime, IDisposable
    {
        #region Constantes e Campos Privados

        // String de conexão para banco SQLite em memória.
        private const string ConnectionString = "Data Source=:memory:";

        // Conexão SQLite que será mantida aberta durante os testes.
        private readonly SqliteConnection _connection;

        // Flag para detectar chamadas redundantes do Dispose.
        private bool _disposed;

        #endregion

        #region Construtor

        /// <summary>
        /// Inicializa uma nova instância da classe EfSqliteFixture.
        /// Configura a conexão SQLite em memória e cria o contexto do Entity Framework.
        /// </summary>
        public EfSqliteFixture()
        {
            // Cria e abre a conexão SQLite em memória
            _connection = new SqliteConnection(ConnectionString);
            _connection.Open();

            // Configura o DbContext para usar SQLite com a conexão criada
            var builder = new DbContextOptionsBuilder<WriteDbContext>().UseSqlite(_connection);
            Context = new WriteDbContext(builder.Options);
        }

        #endregion

        #region Propriedades Públicas

        /// <summary>
        /// Contexto do Entity Framework configurado para usar SQLite em memória.
        /// Utilizado pelos testes para interação com o banco de dados.
        /// </summary>
        public WriteDbContext Context { get; }

        #endregion

        #region Implementação IAsyncLifetime

        /// <summary>
        /// Inicializa o ambiente de teste de forma assíncrona.
        /// Remove o banco existente (se houver) e cria um novo schema limpo.
        /// </summary>
        /// <returns>Task representando a operação assíncrona</returns>
        public async Task InitializeAsync()
        {
            // Garante que o banco seja deletado antes de criar um novo
            await Context.Database.EnsureDeletedAsync();

            // Cria o schema do banco de dados com base no modelo
            await Context.Database.EnsureCreatedAsync();
        }

        /// <summary>
        /// Limpa o ambiente após a execução dos testes.
        /// Implementação vazia pois a limpeza é feita no Dispose.
        /// </summary>
        /// <returns>Task completada</returns>
        public Task DisposeAsync() => Task.CompletedTask;

        #endregion

        #region Implementação IDisposable

        /// <summary>
        /// Finalizer que garante a liberação de recursos não gerenciados.
        /// </summary>
        ~EfSqliteFixture() => Dispose(false);

        /// <summary>
        /// Implementação pública do padrão Dispose.
        /// Libera todos os recursos e suprime a finalização.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Implementação protegida do padrão Dispose.
        /// Libera recursos gerenciados e não gerenciados conforme necessário.
        /// </summary>
        /// <param name="disposing">
        /// True se chamado pelo método Dispose(); false se chamado pelo finalizer
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            // Evita múltiplas chamadas de dispose
            if (_disposed)
                return;

            // Libera recursos gerenciados (objetos que implementam IDisposable)
            if (disposing)
            {
                _connection?.Dispose();
                Context?.Dispose();
            }

            // Marca como disposed para evitar chamadas futuras
            _disposed = true;
        }

        #endregion
    }
}

/*
🧪 EXPLICAÇÃO DA CLASSE EfSqliteFixture

📋 PROPÓSITO:
Fixture de teste que cria um ambiente isolado com SQLite em memória para testes
unitários do Entity Framework, garantindo execução rápida e independente.

🔧 CARACTERÍSTICAS PRINCIPAIS:
• BANCO EM MEMÓRIA: SQLite na RAM para velocidade máxima
• ISOLAMENTO: Cada teste executa com banco limpo e independente  
• CICLO DE VIDA: Gerenciamento automático via IAsyncLifetime e IDisposable
• INTEGRAÇÃO: Compatible com xUnit e Entity Framework

⚡ FLUXO DE EXECUÇÃO:
1. Construtor → Cria conexão SQLite + DbContext
2. InitializeAsync() → Recria schema limpo
3. Teste executa → Usa Context para operações
4. Dispose() → Libera recursos automaticamente

🚀 VANTAGENS:
- Testes ultrarrápidos (memória vs disco)
- Zero configuração de banco externo
- Isolamento total entre testes
- Gerenciamento automático de recursos
- Compatibilidade completa com EF Core

💡 USO TÍPICO:
```csharp
public class MeuTeste : IClassFixture<EfSqliteFixture>
{
    private readonly EfSqliteFixture _fixture;
    
    public MeuTeste(EfSqliteFixture fixture) => _fixture = fixture;
    
    [Fact]
    public async Task DeveSalvar()
    {
        _fixture.Context.Add(entidade);
        await _fixture.Context.SaveChangesAsync();
        // Assert...
    }
}
```
*/