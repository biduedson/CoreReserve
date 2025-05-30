using Microsoft.EntityFrameworkCore;

namespace CoreReserve.Infrastructure.Data.Extensions
{
    /// <summary>
    /// Extensões para configuração do <see cref="ModelBuilder"/> no Entity Framework Core.
    /// </summary>
    internal static class ModelBuilderExtensions
    {
        /// <summary>
        /// Remove a convenção de deleção em cascata do <see cref="ModelBuilder"/>.
        /// </summary>
        /// <param name="modelBuilder">O construtor do modelo.</param>
        internal static void RemoveCascadeDeleteConvention(this ModelBuilder modelBuilder)
        {
            // Obtém todas as chaves estrangeiras no modelo que não são propriedades de posse e possuem comportamento de deleção em cascata
            var foreignKeys = modelBuilder.Model
                .GetEntityTypes()
                .SelectMany(entity => entity.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade)
                .ToList();

            // Altera o comportamento da deleção de cada chave estrangeira para restrito
            foreach (var fk in foreignKeys)
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe ModelBuilderExtensions → Contém extensões para o Entity Framework Core, permitindo configuração personalizada do modelo de dados. 
    ✅ Método RemoveCascadeDeleteConvention() → Impede que chaves estrangeiras excluam automaticamente registros relacionados via deleção em cascata. 
    ✅ Uso de LINQ para filtrar chaves estrangeiras → Identifica somente aquelas que possuem comportamento de deleção automática. 
    ✅ Alteração para DeleteBehavior.Restr() → Impede que chict → Garante que a deleção seja restrita e não remova registros dependentes automaticamenteaves estrangeiras excluam automaticamente registros relacionados via deleção em cascata. 
    ✅ Uso de LINQ para filtrar chaves estrangeiras → Identifica somente aquelas que possuem comportamento de deleção automática. 
    ✅ Alteração para DeleteBehavior.Restrict → Garante que a deleção seja restrita e não remova registros dependentes automaticamente. 
    ✅ Essa abordagem evita erros inesperados de deleção e melhora o controle. 
    ✅ Essa abordagem evita erros inesperados de deleção e melhora o controle sobre a integridade dos dados no banco de dados. 
    */
}