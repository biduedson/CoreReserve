
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Query.Application.User.Queries;
using CoreReserve.Query.Data.Repositories.Abstractions;
using CoreReserve.Query.QueriesModel;
using FluentValidation;
using MediatR;

namespace CoreReserve.Query.Application.User.Handlers
{

    /// <summary>
    /// Manipulador da consulta para obter um usuário por ID.
    /// Responsável por validar a requisição, consultar o banco de dados e otimizar o acesso utilizando cache.
    /// </summary>
    public class GetUserByIdQueryHandler(
        IValidator<GetUserByIdQuery> validator,
        IUserReadOnlyRepository repository,
        ICacheService cacheService) : IRequestHandler<GetUserByIdQuery, Result<UserQueryModel>>
    {
        /// <summary>
        /// Manipula a consulta para obter um usuário pelo identificador.
        /// Valida os dados da requisição, busca no cache e consulta no banco se necessário.
        /// </summary>
        /// <param name="request">Consulta para recuperação de usuário.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>Modelo de usuário encapsulado no resultado.</returns>
        public async Task<Result<UserQueryModel>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            // Valida a requisição antes de executar a consulta.
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                // Retorna o resultado com erros de validação.
                return Result<UserQueryModel>.Invalid(validationResult.AsErrors());
            }

            // Cria uma chave de cache baseada na consulta e no ID do usuário.
            var cacheKey = $"{nameof(GetUserByIdQuery)}_{request.Id}";

            // Obtém o usuário do cache ou busca no banco se não estiver armazenado.
            var user = await cacheService.GetOrCreateAsync(cacheKey, () => repository.GetByIdAsync(request.Id));

            // Se o usuário não for encontrado, retorna um erro indicando que não há registros correspondentes.
            return user == null
                ? Result<UserQueryModel>.NotFound($"No user found with Id: {request.Id}")
                : Result<UserQueryModel>.Success(user);
        }
    }
}
// -----------------------------------------
// 🔹 EXPLICAÇÃO DETALHADA 🔹
// -----------------------------------------

/*
✅ Classe GetUserByIdQueryHandler → Processa a consulta para obter um usuário pelo identificador.
✅ Implementação de IRequestHandler<GetUserByIdQuery, Result<UserQueryModel>> → Define o contrato da consulta com MediatR.
✅ Uso de FluentValidation → Garante que os dados da requisição sejam válidos antes de executar a busca.
✅ Uso de IUserReadOnlyRepository → Garante que os dados sejam recuperados sem modificações, seguindo CQRS.
✅ Uso de ICacheService → Otimiza a recuperação de dados, reduzindo consultas ao banco e melhorando performance.
✅ Chave de cache personalizada → Permite armazenar resultados e reaproveitar dados sem precisar consultar o banco toda vez.
✅ Método Handle() → Garante que a busca seja eficiente e validada antes da execução.
✅ Arquitetura baseada em CQRS → Mantém separação entre comandos e consultas, garantindo escalabilidade e eficiência na recuperação de dados.
✅ Essa abordagem melhora a performance, reduz o tempo de resposta e evita acessos desnecessários ao banco de dados.
*/