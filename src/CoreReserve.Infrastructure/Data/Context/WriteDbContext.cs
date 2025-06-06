using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Infrastructure.Data.Mappings;
using Microsoft.EntityFrameworkCore;

namespace CoreReserve.Infrastructure.Data.Context
{
    /// <summary>
    /// Contexto do banco de dados para operações de escrita.
    /// Responsável por gerenciar e armazenar entidades do domínio.
    /// </summary>
    public class WriteDbContext(DbContextOptions<WriteDbContext> dbOptions)
        : BaseDbContext<WriteDbContext>(dbOptions)
    {
        /// <summary>
        /// Representa o conjunto de clientes armazenados no banco de dados.
        /// </summary>
        public DbSet<User> Users => Set<User>();

        /// <summary>
        /// Configura o modelo do banco de dados, aplicando as configurações da entidade User.
        /// </summary>
        /// <param name="modelBuilder">O construtor do modelo.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe WriteDbContext → Define o contexto de banco de dados para operações de escrita. 
    ✅ Herança de BaseDbContext → Reutiliza configurações globais do banco de dados. 
    ✅ Propriedade Users → Define um conjunto do Entity Framework Core para manipulação de entidades User. 
    ✅ Método OnModelCreating() → Aplica a configuração da entidade User por meio da classe CustomerConfiguration. 
    ✅ Esse contexto permite a gestão eficiente de clientes, centralizando regras de persistência e garantindo consistência dos dados. 
    */

}