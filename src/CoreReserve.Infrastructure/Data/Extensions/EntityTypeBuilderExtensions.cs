using CoreReserve.Core.SharedKernel;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreReserve.Infrastructure.Data.Extensions
{
    /// <summary>
    /// Extensões para configuração de entidades dentro do Entity Framework Core.
    /// </summary>
    internal static class EntityTypeBuilderExtensions
    {
        /// <summary>
        /// Configura a entidade base para um tipo de entidade específico.
        /// </summary>
        /// <typeparam name="TEntity">O tipo da entidade.</typeparam>
        /// <param name="builder">O construtor de entidade.</param>
        internal static void ConfigureBaseEntity<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : BaseEntity
        {
            // Define a chave primária da entidade como a propriedade Id
            builder
                .HasKey(entity => entity.Id);

            // Configura a propriedade Id como obrigatória (NOT NULL) e impede que seja gerada automaticamente pelo banco
            builder
                .Property(entity => entity.Id)
                .IsRequired()
                .ValueGeneratedNever();

            // Ignora a propriedade DomainEvents da entidade
            builder
                .Ignore(entity => entity.DomainEvents);
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe EntityTypeBuilderExtensions → Adiciona métodos de extensão para configuração de entidades no Entity Framework Core. 
    ✅ Método ConfigureBaseEntity<T>() → Define configurações padrão para entidades baseadas em BaseEntity. 
    ✅ Propriedade Id → Configurada como chave primária, obrigatória e sem geração automática, garantindo consistência na identificação dos registros. 
    ✅ Propriedade DomainEvents → Ignorada no mapeamento para evitar persistência desnecessária de eventos no banco. 
    ✅ Essa abordagem centraliza configurações padrão, garantindo uniformidade e reduzindo redundâncias na definição de entidades. 
    */

}