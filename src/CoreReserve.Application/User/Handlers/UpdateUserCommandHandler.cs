using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CoreReserve.Application.User.Commands;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Domain.Entities.UserAggregate;
using CoreReserve.Domain.ValueObjects;
using FluentValidation;
using MediatR;
namespace CoreReserve.Application.User.Handlers
{
    /// <summary>
    /// Manipulador do comando <see cref="UpdateUserCommand"/>.
    /// Responsável por validar, localizar e atualizar um cliente.
    /// </summary>
    public class UpdateUserCommandHandler(
        IValidator<UpdateUserCommand> validator,
        IUserWriteOnlyRepository repository,
        IUnitOfWork unitOfWork) : IRequestHandler<UpdateUserCommand, Result>
    {
        /// <summary>
        /// Processa a atualização do e-mail de um cliente com base no comando recebido.
        /// </summary>
        /// <param name="request">O comando contendo os novos dados do cliente.</param>
        /// <param name="cancellationToken">Token para cancelamento assíncrono.</param>
        /// <returns>Resultado da operação.</returns>
        public async Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
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

            // Instancia o objeto de valor Email.
            var emailResult = Email.Create(request.Email);
            if (!emailResult.IsSuccess)
                return Result.Error(new ErrorList(emailResult.Errors.ToArray()));

            // Verifica se já existe outro cliente com o mesmo e-mail.
            if (await repository.ExistsByEmailAsync(emailResult.Value, user.Id))
                return Result.Error("The email address you provided is already in use.");

            // Atualiza o e-mail na entidade.
            user.ChangeEmail(emailResult.Value);

            // Atualiza a entidade no repositório.
            repository.Update(user);

            // Salva as mudanças no banco de dados e dispara eventos relacionados.
            await unitOfWork.SaveChangesAsync();

            // Retorna a mensagem de sucesso.
            return Result.SuccessWithMessage("Updated successfully!");
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe UpdateUserCommandHandler → Processa o comando para atualização de um cliente. 
    ✅ Uso de FluentValidation → Garante que os dados da requisição sejam válidos antes da execução. 
    ✅ Implementação de IUnitOfWork → Gerencia transações para persistência segura das operações. 
    ✅ Verificação da existência do cliente → Impede erros ao tentar atualizar registros inexistentes. 
    ✅ Verificação de e-mail duplicado → Garante que não haja clientes com e-mails repetidos. 
    ✅ Atualização do e-mail → Modifica a propriedade no banco e mantém consistência dos dados. 
    ✅ Essa abordagem promove segurança e consistência na atualização de clientes dentro do sistema. 
    */

}