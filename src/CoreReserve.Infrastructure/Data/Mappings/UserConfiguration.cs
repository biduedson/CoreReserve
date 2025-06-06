using CoreReserve.Domain.Entities.UserAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoreReserve.Infrastructure.Data.Extensions;

namespace CoreReserve.Infrastructure.Data.Mappings
{
    /// <summary>
    /// Configuração do mapeamento da entidade <see cref="Customer"/> no banco de dados.
    /// Define propriedades, restrições e mapeamento de valores para garantir integridade dos dados.
    /// </summary>
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        /// <summary>
        /// Configura o mapeamento da entidade Customer.
        /// </summary>
        /// <param name="builder">O construtor de entidade.</param>
        public void Configure(EntityTypeBuilder<User> builder)
        {
            // Configura a entidade base
            builder
                .ConfigureBaseEntity();

            // Configura Name como obrigatório e define um tamanho máximo de 100 caracteres
            builder
                .Property(user => user.Name)
                .IsRequired() // NOT NULL
                .HasMaxLength(100);

            // Configura Gender como obrigatório e converte para string com limite de 6 caracteres
            builder
                .Property(user => user.Gender)
                .IsRequired() // NOT NULL
                .HasMaxLength(6)
                .HasConversion<string>();

            // Mapeamento do Value Object Email dentro da entidade User
            builder.OwnsOne(user => user.Email, ownedNav =>
            {
                ownedNav
                    .Property(email => email.Address)
                    .IsRequired() // NOT NULL
                    .HasMaxLength(254)
                    .HasColumnName(nameof(User.Email));

                // Define um índice único para garantir que não existam e-mails duplicados
                ownedNav
                    .HasIndex(email => email.Address)
                    .IsUnique();
            });

            // Mapeamento do Value Object Password dentro da entidade User
            builder.OwnsOne(user => user.Password, ownedNav =>
            {
                ownedNav
                .Property(password => password.NewPassword)
                .IsRequired() // NOT NULL
                .HasMaxLength(255)
                .HasColumnName(nameof(User.Password));

            });

            // Configura CreatedAt como obrigatório e define o tipo da coluna como DATE
            builder
                .Property(user => user.CreatedAt)
                .IsRequired() // NOT NULL
                .HasColumnType("DATE");
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe UserConfiguration → Define o mapeamento da entidade User no banco de dados. 
    ✅ Método Configure() → Aplica configurações, incluindo obrigatoriedade e limites de tamanho para propriedades. 
    ✅ Propriedade Gender → Converte para string, garantindo compatibilidade e consistência no armazenamento. 
    ✅ Mapeamento do Value Object Email → Usa OwnsOne para definir Email como um objeto de valor dentro da entidade. 
    ✅ Propriedade Email.Address → Configurada como única, garantindo que não haja duplicidade de e-mails na base de dados. 
    ✅ Propriedade CreatedAt → Armazenada como DATE no banco para garantir precisão de tempo. 
    ✅ Essa abordagem melhora a integridade dos dados e facilita consultas eficientes no sistema. 
    */

}