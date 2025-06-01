using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using CoreReserve.Core.Extensions;


namespace CoreReserve.Application.Behaviors
{
    /// <summary>
    /// Comportamento de pipeline que adiciona logging ao processamento de comandos no MediatR.
    /// </summary>
    /// <typeparam name="TRequest">O tipo da requisição.</typeparam>
    /// <typeparam name="TResponse">O tipo da resposta.</typeparam>
    public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        /// <summary>
        /// Intercepta o processamento de uma requisição, adicionando logs antes e depois da execução.
        /// </summary>
        /// <param name="request">A requisição a ser processada.</param>
        /// <param name="next">Delegate para invocar o próximo comportamento no pipeline.</param>
        /// <param name="cancellationToken">Token de cancelamento.</param>
        /// <returns>A resposta da requisição.</returns>
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var commandName = request.GetGenericTypeName();

            logger.LogInformation("----- Iniciando processamento do comando '{CommandName}'", commandName);

            var timer = new Stopwatch();
            timer.Start();

            var response = await next(cancellationToken);

            timer.Stop();

            var timeTaken = timer.Elapsed.TotalSeconds;
            logger.LogInformation("----- Comando '{CommandName}' processado ({TimeTaken} segundos)", commandName, timeTaken);

            return response;
        }
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Classe LoggingBehavior → Implementa um comportamento de pipeline para registrar logs durante a execução de comandos no MediatR. 
    ✅ Método Handle() → Intercepta a requisição e adiciona logs antes e depois do processamento, medindo o tempo de execução. 
    ✅ Uso de Stopwatch → Mede o tempo total de processamento do comando, permitindo diagnóstico de performance. 
    ✅ Uso de ILogger → Registra mensagens informativas no log, facilitando depuração e rastreabilidade. 
    ✅ Essa abordagem melhora a visibilidade sobre a execução dos comandos, facilitando auditoria e análise de desempenho. 
    */
}