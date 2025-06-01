namespace CoreReserve.Application.User.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using Ardalis.Result;
    using Ardalis.Result.FluentValidation;
    using CoreReserve.Application.User.Commands;
    using CoreReserve.Core.SharedKernel;
    using CoreReserve.Domain.Entities.UserAggregate;
    using FluentValidation;
    using MediatR;


    /// <summary>
    /// Manipulador do comando <see cref="DeleteUserCommand"/>.
    /// Responsável por validar, localizar e excluir um cliente.
    /// </summary>
    public class DeleteUserCommandHandler(
        IValidator<DeleteUserCommand> validator,
        IUserWriteOnlyRepository repository,
        IUnitOfWork unitOfWork) : IRequestHandler<DeleteUserCommand, Result>
    {
        /// <summary>
        /// Processa a remoção de um cliente com base no comando recebido.
        /// </summary>
        /// <param name="request">O comando contendo o ID do cliente.</param>
        /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
        /// <returns>Resultado da operação.</returns>
        public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            // Valida a requisição.
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                // Retorna resultado com erros de validação.
                return Result.Invalid(validationResult.AsErrors());
            }

            // Recupera o cliente do banco de dados.
            var user = await repository.GetByIdAsync(request.Id);
            if (user == null)
                return Result.NotFound($"No customer found with Id: {request.Id}");

            // Marca a entidade como excluída, gerando o evento CustomerDeletedEvent.
            user.Delete();

            // Remove a entidade do repositório.
            repository.Remove(user);

            // Salva as mudanças no banco de dados e dispara eventos relacionados.
            await unitOfWork.SaveChangesAsync();

            // Retorna a mensagem de sucesso.
            return Result.SuccessWithMessage("Removed successfully!");
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe DeleteUserCommandHandler → Processa o comando para remoção de um cliente. 
    ✅ Uso de FluentValidation → Garante que os dados da requisição sejam válidos antes da execução. 
    ✅ Implementação de IUnitOfWork → Gerencia transações para persistência segura das operações. 
    ✅ Verificação da existência do cliente → Impede erros ao tentar excluir registros inexistentes. 
    ✅ Geração automática do evento "CustomerDeletedEvent" → Permite rastrear operações realizadas sobre clientes. 
    ✅ Essa abordagem promove segurança e consistência na exclusão de clientes dentro do sistema. 
    */

}