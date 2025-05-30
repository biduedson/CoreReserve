using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using CoreReserve.Infrastructure.Data.Extensions;
namespace CoreReserve.Infrastructure.Data.Context
{
    /// <summary>
    /// Classe base para contextos do Entity Framework Core, garantindo configurações comuns para o banco de dados.
    /// </summary>
    public abstract class BaseDbContext<TContext>(DbContextOptions<TContext> dbOptions) : DbContext(dbOptions)
        where TContext : DbContext
    {
        /// <summary>
        /// Define a collation padrão para o banco de dados.
        /// Essa configuração garante que o banco de dados ignore diferenças de acentuação e caixa ao comparar strings.
        /// </summary>
        private const string Collation = "Latin1_General_CI_AI";

        /// <summary>
        /// Configura o rastreamento de alterações do Entity Framework Core.
        /// Desativa carregamento lento e define o comportamento da deleção em cascata.
        /// </summary>
        public override ChangeTracker ChangeTracker
        {
            get
            {
                base.ChangeTracker.LazyLoadingEnabled = false;
                base.ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
                base.ChangeTracker.DeleteOrphansTiming = CascadeTiming.OnSaveChanges;
                return base.ChangeTracker;
            }
        }

        /// <summary>
        /// Define convenções globais de mapeamento para as entidades.
        /// </summary>
        /// <param name="configurationBuilder">O construtor de configuração do modelo.</param>
        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder
                .Properties<string>()
                .AreUnicode(false)
                .HaveMaxLength(255);

            base.ConfigureConventions(configurationBuilder);
        }

        /// <summary>
        /// Configura o modelo ao criar o contexto do banco de dados.
        /// Define collation e remove a convenção de deleção em cascata.
        /// </summary>
        /// <param name="modelBuilder">O construtor do modelo.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .UseCollation(Collation)
                .RemoveCascadeDeleteConvention();

            base.OnModelCreating(modelBuilder);
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe BaseDbContext → Define um contexto de banco de dados base para ser reutilizado por outros contextos. 
    ✅ Propriedade ChangeTracker → Configura o rastreamento de mudanças no Entity Framework Core, desativando carregamento lento e ajustando regras de deleção. 
    ✅ Método ConfigureConventions() → Define que todas as propriedades string não devem ser Unicode e devem ter um tamanho máximo de 255 caracteres. 
    ✅ Método OnModelCreating() → Configura a collation do banco e remove a convenção de deleção em cascata para evitar remoções inesperadas. 
    ✅ Utilizar um contexto base como esse é útil para centralizar configurações globais, garantindo consistência e reduzindo redundâncias no código. 
    */
}