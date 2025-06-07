using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using System.Threading.Tasks;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CoreReserve.Application;
using CoreReserve.Infrastructure;
using CoreReserve.Query;
using CoreReserve.Application.User.Responses;
using CoreReserve.PublicApi.Models;
using CoreReserve.Application.User.Commands;
using CoreReserve.PublicApi.Extensions;
using CoreReserve.Query.QueriesModel;
using CoreReserve.Query.Application.User.Queries;


namespace CoreReserve.PublicApi.Controllers.v1
{
    ////////////////////////
    // POST: /api/users
    ////////////////////////

    /// /// <summary>
    /// Registra um novo cliente.
    /// </summary>
    /// <response code="201">Retorna o Id do novo usuario.</response>
    /// <response code="400">Retorna lista de erros se a requisição for inválida.</response>
    /// <response code="500">Quando ocorre um erro interno inesperado no servidor.</response>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class UsersController(IMediator mediator) : ControllerBase
    {
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse<CreatedUserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody][Required] CreateUserCommand command) =>
                 (await mediator.Send(command)).ToActionResult();

        /// <summary>
        /// Exclui o usuario pelo Id.
        /// </summary>
        /// <response code="200">Retorna a resposta com a mensagem de sucesso.</response>
        /// <response code="400">Retorna lista de erros se a requisição for inválida.</response>
        /// <response code="404">Quando nenhum usuario é encontrado pelo Id fornecido.</response>
        /// <response code="500">Quando ocorre um erro interno inesperado no servidor.</response>
        [HttpDelete("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([Required] Guid id) =>
            (await mediator.Send(new DeleteUserCommand(id))).ToActionResult();

        ///////////////////////////
        // GET: /api/users/{id}
        ///////////////////////////

        /// <summary>
        /// Obtém o usuario pelo Id.
        /// </summary>
        /// <response code="200">Retorna o usuario.</response>
        /// <response code="400">Retorna lista de erros se a requisição for inválida.</response>
        /// <response code="404">Quando nenhum cliente é encontrado pelo Id fornecido.</response>
        /// <response code="500">Quando ocorre um erro interno inesperado no servidor.</response>
        [HttpGet("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse<UserQueryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([Required] Guid id) =>
            (await mediator.Send(new GetUserByIdQuery(id))).ToActionResult();

        //////////////////////
        // GET: /api/users
        //////////////////////

        /// <summary>
        /// Obtém uma lista de todos os usuarios.
        /// </summary>
        /// <response code="200">Retorna a lista de usuarios.</response>
        /// <response code="500">Quando ocorre um erro interno inesperado no servidor.</response>
        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll() =>
            (await mediator.Send(new GetAllUserQuery())).ToActionResult();
    }
}

/*
 * EXPLICAÇÃO DO CÓDIGO:
 * 
 * Este é um controller de API REST para gerenciar Usuarios (Users) em uma aplicação ASP.NET Core.
 * 
 * CARACTERÍSTICAS PRINCIPAIS:
 * - Usa o padrão CQRS (Command Query Responsibility Segregation) através do MediatR
 * - Implementa versionamento de API (versão 1.0) com Asp.Versioning
 * - Segue as convenções REST para operações CRUD
 * 
 * ESTRUTURA:
 * 1. [ApiController] - Indica que é um controller de API com validações automáticas
 * 2. [ApiVersion("1.0")] - Define a versão da API
 * 3. [Route("api/[controller]")] - Define a rota base como /api/users
 * 4. Injeção de dependência do IMediator via construtor primário (C# 12)
 * 
 * MÉTODOS IMPLEMENTADOS:
 * 
 * 1. CREATE (POST /api/users)
 *    - Recebe um CreateUserCommand no body da requisição
 *    - Retorna 201 com o ID do novo usuario ou 400/500 para erros
 * 
 * 2. UPDATE (PUT /api/users)
 *    - Recebe um UpdateUsersCommand no body
 *    - Retorna 200 para sucesso, 400 para dados inválidos, 404 se não encontrar o cliente
 * 
 * 3. DELETE (DELETE /api/users/{id})
 *    - Recebe o GUID do usuario na URL
 *    - Cria automaticamente o DeleteUserrCommand com o ID
 *    - Retorna 200 para sucesso, 400/404/500 para erros
 * 
 * 4. GET BY ID (GET /api/users/{id})
 *    - Recebe o GUID do usuario na URL
 *    - Retorna o UserQueryModel encontrado ou 404 se não existir
 * 
 * 5. GET ALL (GET /api/users)
 *    - Lista todos os usuarios
 *    - Retorna uma coleção de UserQueryModel
 * 
 * PADRÕES UTILIZADOS:
 * - CQRS: Separação entre Commands (escrita) e Queries (leitura)
 * - Mediator: Desacopla o controller da lógica de negócio
 * - Extension Methods: .ToActionResult() converte responses para IActionResult
 * - Async/Await: Todas as operações são assíncronas
 * - Data Annotations: [Required] para validação de parâmetros
 * - Content Negotiation: Define tipos de conteúdo consumidos e produzidos (JSON)
 * 
 * BENEFÍCIOS:
 * - Código limpo e desacoplado
 * - Fácil manutenção e testabilidade
 * - Documentação automática com Swagger através dos atributos ProducesResponseType
 * - Validações automáticas do ASP.NET Core Model Binding
 * - Versionamento de API para evolução sem quebrar clientes existentes
 */