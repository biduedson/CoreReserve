using CoreReserve.Query.Abstractions;
using CoreReserve.Query.QueriesModel;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace CoreReserve.Query.Data.Mappings;

/// <summary>
/// Mapeamento do modelo de consulta de usuários para o banco de dados MongoDB.
/// Garante a estrutura correta dos dados armazenados, incluindo tipos e regras de serialização.
/// </summary>
public class UserMap : IReadDbMapping
{
    /// <summary>
    /// Configura o mapeamento do modelo de usuários no MongoDB.
    /// Define regras de serialização e garante que todos os campos necessários sejam corretamente registrados.
    /// </summary>
    public void Configure()
    {
        // Registra um mapeamento para a classe UserQueryModel, caso ainda não esteja registrado.
        BsonClassMap.TryRegisterClassMap<UserQueryModel>(classMap =>
        {
            classMap.AutoMap(); // Mapeia automaticamente as propriedades da classe.
            classMap.SetIgnoreExtraElements(true); // Ignora elementos extras não mapeados para evitar erros.

            // Define que os membros abaixo são obrigatórios:
            classMap.MapMember(user => user.Id)
                  .SetIsRequired(true);

            classMap.MapMember(user => user.Name)
                  .SetIsRequired(true);

            classMap.MapMember(user => user.Gender)
                  .SetIsRequired(true);

            classMap.MapMember(user => user.Email)
                  .SetIsRequired(true);

            classMap.MapMember(user => user.CreatedAt)
               .SetIsRequired(true)
               .SetSerializer(new DateTimeSerializer(true)); // Serializa a data corretamente para o MongoDB.
        });
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe UserMap → Define o mapeamento do modelo de consulta de usuários para o banco de dados MongoDB.
✅ Implementação de IReadDbMapping → Interface para configuração de mapeamentos no banco.
✅ Método Configure() → Responsável por definir como os objetos UserQueryModel serão armazenados e acessados.
✅ Uso de AutoMap() → Mapeia automaticamente todas as propriedades do modelo de usuário.
✅ Uso de SetIgnoreExtraElements(true) → Ignora campos extras para evitar erros na desserialização de objetos.
✅ Configuração de campos obrigatórios → Garante que os dados essenciais (ID, nome, gênero, e-mail, senha e data de criação) estejam sempre presentes.
✅ Serialização correta do campo CreatedAt → Define a estrutura de armazenamento da data no MongoDB.
✅ Arquitetura baseada em CQRS → Mantém separação entre leitura e escrita, garantindo escalabilidade e organização.
✅ Essa abordagem melhora a integridade dos dados e facilita a manutenção do banco de usuários.
*/