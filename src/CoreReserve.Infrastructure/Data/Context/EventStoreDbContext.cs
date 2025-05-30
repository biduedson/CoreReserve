using CoreReserve.Core.SharedKernel;
using CoreReserve.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace CoreReserve.Infrastructure.Data.Context
{
    /// <summary>
    /// Contexto do banco de dados para armazenar eventos no Event Store.
    /// </summary>
    public class EventStoreDbContext(DbContextOptions<EventStoreDbContext> dbOptions)
        : BaseDbContext<EventStoreDbContext>(dbOptions)
    {
        /// <summary>
        /// Representa o conjunto de eventos armazenados no banco de dados.
        /// </summary>
        public DbSet<EventStore> EventStores => Set<EventStore>();

        /// <summary>
        /// Configura o modelo do banco de dados, aplicando as configurações do Event Store.
        /// </summary>
        /// <param name="modelBuilder">O construtor do modelo.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new EventStoreConfiguration());
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe EventStoreDbContext → Define o contexto de banco de dados para armazenar eventos no Event Store. 
    ✅ Herança de BaseDbContext → Reutiliza configurações padrão definidas para o banco de dados. 
    ✅ Propriedade EventStores → Define um conjunto do Entity Framework Core para manipulação de eventos armazenados. 
    ✅ Método OnModelCreating() → Aplica a configuração do Event Store por meio da classe EventStoreConfiguration. 
    ✅ Essa abordagem facilita o gerenciamento de eventos e melhora a rastreabilidade de mudanças dentro do sistema. 
    */
}