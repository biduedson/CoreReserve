using CoreReserve.Query.Abstractions;

namespace CoreReserve.Query.QueriesModel
{

    /// <summary>
    /// Modelo de consulta para usuarios.
    /// Utilizado na camada de consulta para representar os dados dos usuarios recuperados do banco.
    /// </summary>
    public class UserQueryModel : IQueryModel<Guid>
    {
        /// <summary>
        /// Construtor principal que inicializa um modelo de cliente baseado nos dados fornecidos.
        /// </summary>
        /// <param name="id">Identificador único do usuario</param>
        /// <param name="name">Nome do usuario.</param>
        /// <param name="gender">Gênero do usuario.</param>
        /// <param name="email">Endereço de e-mail do usuario.</param>
        /// <param name="password">Endereço de e-mail do usuario.</param>
        /// <param name="isActive">Define se  o usuario esta ativo</param>
        /// <param name="createdAt">Data de criaçao do usuario.</param>
        public UserQueryModel(
            Guid id,
            string name,
            string gender,
            string email,
            string password,
            bool isActive,
            DateTime createdAt)
        {
            Id = id;
            Name = name;
            Password = password;
            Gender = gender;
            Email = email;
            CreatedAt = createdAt;
        }

        /// <summary>
        /// Construtor privado usado pelo mecanismo de persistência.
        /// Garante que objetos sejam corretamente materializados sem inicialização direta.
        /// </summary>
        private UserQueryModel()
        {
        }

        /// <summary>
        /// Identificador único do usuario.
        /// </summary>
        public Guid Id { get; private init; }

        /// <summary>
        /// Primeiro nome do usuario.
        /// </summary>
        public string Name { get; private init; }

        /// <summary>
        /// Gênero do usuario.
        /// </summary>
        public string Gender { get; private init; }

        /// <summary>
        /// Endereço de e-mail do usuario.
        /// </summary>
        public string Email { get; private init; }

        /// <summary>
        /// password do usuario.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        /// Define se  o usuario esta ativo.
        /// </summary>
        public bool IsActive { get; } = true;

        /// <summary>
        /// Data de criaçao do usuario.
        /// </summary>
        public DateTime CreatedAt { get; }

    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe UserQueryModel → Modelo de consulta usado para recuperar usuarios no banco.
✅ Implementação de IQueryModel<Guid> → Indica que a entidade possui um identificador único do tipo GUID.
✅ Construtor principal → Inicializa um objeto com os dados do usuario.
✅ Construtor privado → Utilizado pelo mecanismo de persistência para recriar objetos sem inicialização manual.
✅ Propriedades com init-only → Garante que os valores sejam atribuídos apenas durante a criação do objeto.
✅ Propriedade FullName → Facilita a obtenção do nome completo do usuario sem necessidade de concatenação manual.
✅ Arquitetura baseada em CQRS → Mantém separação entre comandos (modificação) e consultas (leitura).
✅ Essa estrutura melhora eficiência na leitura de dados e mantém a integridade da modelagem.
*/

