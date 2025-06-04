using Ardalis.Result;
using CoreReserve.Core.SharedKernel;
using CoreReserve.Query.Application.User.Queries;
using CoreReserve.Query.Data.Repositories.Abstractions;
using CoreReserve.Query.QueriesModel;
using MediatR;

namespace CoreReserve.Query.Application.User.Handlers
{
    /// <summary>
    /// Manipulador da consulta para obter todos os Usuarios.
    /// Responsável por recuperar os dados do banco e otimizar o acesso utilizando cache.
    /// </summary>
    public class GetAllUserQueryHandler(IUserReadOnlyRepository repository, ICacheService cacheService)
        : IRequestHandler<GetAllUserQuery, Result<IEnumerable<UserQueryModel>>>
    {
        /// <summary>
        /// Chave utilizada para armazenar os dados de Usuarios no cache.
        /// </summary>
        private const string CacheKey = nameof(GetAllUserQuery);

        /// <summary>
        /// Manipula a consulta para obter todos os Usuarios.
        /// Retorna os dados armazenados no cache ou recupera do banco caso não estejam cacheados.
        /// </summary>
        /// <param name="request">Consulta para recuperação de Usuarios.</param>
        /// <param name="cancellationToken">Token de cancelamento da operação.</param>
        /// <returns>Lista de Usuarios encapsulada no resultado.</returns>
        public async Task<Result<IEnumerable<UserQueryModel>>> Handle(
              GetAllUserQuery request,
              CancellationToken cancellationToken)
        {
            // Retorna os dados do cache ou recupera do banco caso não estejam disponíveis.
            return Result<IEnumerable<UserQueryModel>>.Success(
                await cacheService.GetOrCreateAsync(CacheKey, repository.GetAllAsync));
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DETALHADA 🔹
    // -----------------------------------------

    /*
    ✅ Classe GetAllCustomerQueryHandler → Processa a consulta para obter todos os Usuarios.
    ✅ Implementação de IRequestHandler<GetAllUserQuery, Result<IEnumerable<UserQueryModel>>> → Define o contrato da consulta com MediatR.
    ✅ Uso de IUserReadOnlyRepository → Garante que os dados sejam recuperados sem modificações, seguindo CQRS.
    ✅ Uso de ICacheService → Otimiza a recuperação de dados, reduzindo consultas ao banco e melhorando performance.
    ✅ Constante CacheKey → Define uma chave fixa para armazenar os dados no cache.
    ✅ Método Handle() → Obtém os dados da fonte mais eficiente (cache ou repositório), garantindo que a resposta seja rápida.
    ✅ Arquitetura baseada em CQRS → Separa operações de consulta e manipulação de dados, garantindo escalabilidade e eficiência.
    ✅ Essa abordagem melhora a performance e reduz o tempo de resposta ao buscar registros no banco.
    */


}