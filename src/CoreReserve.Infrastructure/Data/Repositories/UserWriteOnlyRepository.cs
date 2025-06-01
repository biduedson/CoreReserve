using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.ValueObjects;
using CoreReserve.Infrastructure.Data.Context;
using CoreReserve.Infrastructure.Data.Repositories.Common;
using Microsoft.EntityFrameworkCore;

namespace CoreReserve.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repositório somente de escrita para a entidade <see cref="User"/>.
    /// Permite verificar a existência de clientes com base no e-mail.
    /// </summary>
    internal class UserWriteOnlyRepository(WriteDbContext dbContext)
           : BaseWriteOnlyRepository<User, Guid>(dbContext), IUserWriteOnlyRepository
    {
        /// <summary>
        /// Consulta compilada para verificar se um cliente existe pelo e-mail.
        /// Utiliza uma consulta otimizada para melhorar a performance.
        /// </summary>
        private static readonly Func<WriteDbContext, string, Task<bool>> ExistsByEmailCompiledAsync =
            EF.CompileAsyncQuery((WriteDbContext dbcontext, string email) =>
            dbcontext
                  .Users
                  .AsNoTracking()
                  .Any(user => user.Email.Address == email));

        /// <summary>
        /// Consulta compilada para verificar se um cliente existe pelo e-mail, excluindo um ID específico.
        /// Utilizada para evitar conflitos ao atualizar informações do cliente.
        /// </summary>    
        private static readonly Func<WriteDbContext, string, Guid, Task<bool>> ExistsByEmailAndIdCompiledAsync =
            EF.CompileAsyncQuery((WriteDbContext dbContext, string email, Guid currentId) =>
                dbContext
                      .Users
                      .AsNoTracking()
                      .Any(user => user.Email.Address == email && user.Id != currentId));

        /// <summary>
        /// Verifica se um cliente existe pelo e-mail informado.
        /// </summary>
        /// <param name="email">O e-mail do cliente.</param>
        /// <returns>True se o cliente existir, False caso contrário.</returns>
        public Task<bool> ExistsByEmailAsync(Email email) =>
             ExistsByEmailCompiledAsync(DbContext, email.Address);

        /// <summary>
        /// Verifica se um cliente existe pelo e-mail, excluindo um ID específico.
        /// </summary>
        /// <param name="email">O e-mail do cliente.</param>
        /// <param name="currentId">O ID do cliente a ser excluído da verificação.</param>
        /// <returns>True se o cliente existir com outro ID, False caso contrário.</returns>
        public Task<bool> ExistsByEmailAsync(Email email, Guid currentId) =>
             ExistsByEmailAndIdCompiledAsync(DbContext, email.Address, currentId);
    }
}

// -----------------------------------------
// 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
// -----------------------------------------
/*
✅ Classe CustomerWriteOnlyRepository → Implementa um repositório apenas de escrita para clientes. 
✅ Herança de BaseWriteOnlyRepository → Garante que apenas operações de escrita sejam permitidas. 
✅ Métodos ExistsByEmailAsync() e ExistsByEmailAsync(Email, Guid) → Validam se um e-mail já está registrado, evitando duplicidade. 
✅ Uso de consultas compiladas → Melhora a performance ao evitar processamento desnecessário do Entity Framework Core. 
✅ As verificações por e-mail ajudam a evitar erros e conflitos ao cadastrar ou atualizar clientes. 
*/