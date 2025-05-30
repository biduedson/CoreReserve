using CoreReserve.Core.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreReserve.Infrastructure.Data.Mappings
{
    /// <summary>
    /// Configuração do mapeamento da entidade <see cref="EventStore"/> no banco de dados.
    /// Define propriedades e restrições para garantir a integridade dos dados.
    /// </summary>
    internal class EventStoreConfiguration : IEntityTypeConfiguration<EventStore>
    {
        /// <summary>
        /// Configura o mapeamento da entidade EventStore.
        /// </summary>
        /// <param name="builder">O construtor de entidade.</param>
        public void Configure(EntityTypeBuilder<EventStore> builder)
        {
            // Define a chave primária da entidade
            builder
                .HasKey(eventStore => eventStore.Id);

            // Configura a propriedade Id como obrigatória e impede que seja gerada automaticamente
            builder
                .Property(eventStore => eventStore.Id)
                .IsRequired() // NOT NULL
                .ValueGeneratedNever();

            // Define que o AggregateId é obrigatório
            builder
                .Property(eventStore => eventStore.AggregateId)
                .IsRequired(); // NOT NULL

            // Configura MessageType como obrigatório e limita seu tamanho a 100 caracteres
            builder
                .Property(eventStore => eventStore.MessageType)
                .IsRequired() // NOT NULL
                .HasMaxLength(100);

            // Configura Data como obrigatório e adiciona um comentário explicando que os dados são um JSON serializado
            builder
                .Property(eventStore => eventStore.Data)
                .IsRequired() // NOT NULL
                .HasComment("Evento serializado em JSON");

            // Define OccurredOn como obrigatório e renomeia a coluna para CreatedAt no banco de dados
            builder
                .Property(eventStore => eventStore.OccurredOn)
                .IsRequired() // NOT NULL
                .HasColumnName("CreatedAt");
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe EventStoreConfiguration → Define o mapeamento da entidade EventStore no banco de dados. 
    ✅ Método Configure() → Configura restrições como chaves primárias, obrigatoriedade de propriedades e limite de tamanho. 
    ✅ Propriedades marcadas como IsRequired → Garante que os campos não sejam nulos no banco de dados. 
    ✅ Propriedade Id → Impede que seja gerada automaticamente, assegurando consistência com a lógica de identificação. 
    ✅ Propriedade Data → Armazena eventos serializados em JSON, útil para rastreabilidade e recuperação de informações. 
    ✅ A configuração personalizada melhora a integridade dos dados e facilita auditoria e recuperação de eventos no sistema. 
    */

}