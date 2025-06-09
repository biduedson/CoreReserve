
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using CoreReserve.Application.User.Responses;
using CoreReserve.PublicApi.Models;
using CoreReserve.Application.User.Commands;
using CoreReserve.PublicApi.Extensions;
using CoreReserve.Query.QueriesModel;
using CoreReserve.Query.Application.User.Queries;

namespace CoreReserve.PublicApi.Controllers.v1
{
    /// <summary>
    /// 🧑‍💼 Controller responsável pelo gerenciamento de usuários
    /// Implementa operações CRUD completas seguindo padrões REST e CQRS
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/[controller]")]
    public class UsersController(IMediator mediator) : ControllerBase
    {
        #region 📝 CREATE - Criação de Usuário

        /// <summary>
        /// ✨ Registra um novo usuário no sistema
        /// </summary>
        /// <param name="command">Dados do usuário a ser criado</param>
        /// <returns>ID do usuário criado</returns>
        /// <response code="201">✅ Usuário criado com sucesso - Retorna o ID do novo usuário</response>
        /// <response code="400">❌ Dados inválidos - Retorna lista de erros de validação</response>
        /// <response code="500">💥 Erro interno do servidor - Erro inesperado</response>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse<CreatedUserResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody][Required] CreateUserCommand command) =>
            (await mediator.Send(command)).ToActionResult();

        #endregion

        #region 🗑️ DELETE - Exclusão de Usuário

        /// <summary>
        /// 🗑️ Remove um usuário do sistema pelo ID
        /// </summary>
        /// <param name="id">Identificador único do usuário</param>
        /// <returns>Confirmação da exclusão</returns>
        /// <response code="200">✅ Usuário excluído com sucesso</response>
        /// <response code="400">❌ ID inválido - Formato incorreto</response>
        /// <response code="404">🔍 Usuário não encontrado - ID não existe</response>
        /// <response code="500">💥 Erro interno do servidor - Erro inesperado</response>
        [HttpDelete("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete([Required] Guid id) =>
            (await mediator.Send(new DeleteUserCommand(id))).ToActionResult();

        #endregion

        #region 🔍 READ - Consulta Individual de Usuário

        /// <summary>
        /// 🔍 Obtém os dados de um usuário específico pelo ID
        /// </summary>
        /// <param name="id">Identificador único do usuário</param>
        /// <returns>Dados completos do usuário</returns>
        /// <response code="200">✅ Usuário encontrado - Retorna dados do usuário</response>
        /// <response code="400">❌ ID inválido - Formato incorreto</response>
        /// <response code="404">🔍 Usuário não encontrado - ID não existe</response>
        /// <response code="500">💥 Erro interno do servidor - Erro inesperado</response>
        [HttpGet("{id:guid}")]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse<UserQueryModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([Required] Guid id) =>
            (await mediator.Send(new GetUserByIdQuery(id))).ToActionResult();

        #endregion

        #region 📋 READ ALL - Listagem de Usuários

        /// <summary>
        /// 📋 Obtém uma lista completa de todos os usuários cadastrados
        /// </summary>
        /// <returns>Lista de todos os usuários</returns>
        /// <response code="200">✅ Lista obtida com sucesso - Retorna todos os usuários</response>
        /// <response code="500">💥 Erro interno do servidor - Erro inesperado</response>
        [HttpGet]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<UserQueryModel>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll() =>
            (await mediator.Send(new GetAllUserQuery())).ToActionResult();

        #endregion
    }
}

/*
 * 🎯 EXPLICAÇÃO DETALHADA DO CÓDIGO
 * ================================
 * 
 * 📊 VISÃO GERAL:
 * Este controller implementa uma API REST completa para gerenciamento de usuários,
 * seguindo as melhores práticas de desenvolvimento moderno em .NET.
 * 
 * 🏗️ ARQUITETURA IMPLEMENTADA:
 * ┌─────────────────────────────────────────────────────────────┐
 * │                    🎯 PADRÕES UTILIZADOS                    │
 * ├─────────────────────────────────────────────────────────────┤
 * │ 🔄 CQRS (Command Query Responsibility Segregation)         │
 * │ 📡 Mediator Pattern - Desacoplamento total                 │
 * │ 🎭 Repository Pattern - Abstração de dados                 │
 * │ 🔧 Dependency Injection - Inversão de controle             │
 * │ 📝 REST API - Padrões HTTP semânticos                      │
 * │ 🏷️ API Versioning - Evolução sem quebrar compatibilidade   │
 * └─────────────────────────────────────────────────────────────┘
 * 
 * 🎨 ESTRUTURA DO CONTROLLER:
 * ├── 📝 CREATE (POST)   → Criação de novos usuários
 * ├── 🗑️ DELETE (DELETE) → Remoção de usuários existentes
 * ├── 🔍 READ (GET/{id}) → Consulta individual por ID
 * └── 📋 READ ALL (GET)  → Listagem completa de usuários
 * 
 * 🔧 RECURSOS TÉCNICOS:
 * ┌─────────────────────────────────────────────────────────────┐
 * │ 🚀 C# 12 - Primary Constructors                            │
 * │ ⚡ Async/Await - Operações não-bloqueantes                  │
 * │ 🛡️ Data Annotations - Validações automáticas               │
 * │ 📄 Swagger/OpenAPI - Documentação automática               │
 * │ 🎯 Content Negotiation - Suporte a JSON                    │
 * │ 🔍 Strong Typing - Tipagem forte em responses              │
 * └─────────────────────────────────────────────────────────────┘
 * 
 * 💡 BENEFÍCIOS DA IMPLEMENTAÇÃO:
 * ┌─────────────────────────────────────────────────────────────┐
 * │ ✅ Código limpo e organizad                                │
 * │ 🧪 Alta testabilidade                                      │
 * │ 🔄 Fácil manutenção                                        │
 * │ 📈 Escalabilidade                                          │
 * │ 🛡️ Tratamento robusto de erros                             │
 * │ 📚 Documentação automática                                 │
 * │ 🎯 Separação clara de responsabilidades                    │
 * └─────────────────────────────────────────────────────────────┘
 * 
 * 🌟 HIGHLIGHTS ESPECIAIS:
 * • Extension Methods personalizados (.ToActionResult())
 * • Validação automática via Model Binding
 * • Responses tipados para melhor IntelliSense
 * • Códigos de status HTTP semânticos
 * • Organização em regions para melhor navegação
 * • Documentação rica com emojis para facilitar compreensão
 * 
 * 🔮 FUTURAS EXPANSÕES POSSÍVEIS:
 * • Implementação de UPDATE (PUT/PATCH)
 * • Paginação para GetAll
 * • Filtros e ordenação
 * • Cache de consultas
 * • Rate limiting
 * • Autenticação e autorização
 */