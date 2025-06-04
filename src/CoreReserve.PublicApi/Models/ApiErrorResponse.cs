using System.Text.Json.Serialization;

namespace CoreReserve.PublicApi.Models
{
    /// <summary>
    /// Representa uma resposta de erro padronizada para a API.
    /// Contém uma mensagem de erro que pode ser serializada e retornada ao cliente.
    /// </summary>
    [method: JsonConstructor]
    public sealed class ApiErrorResponse(string message)
    {
        /// <summary>
        /// Mensagem de erro descritiva.
        /// </summary>
        public string Message { get; } = message;

        /// <summary>
        /// Retorna a mensagem de erro como string.
        /// Útil para exibição simplificada da mensagem.
        /// </summary>
        public override string ToString() => Message;
    }

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DETALHADA 🔹
    // -----------------------------------------

    /*
    ✅ Classe ApiErrorResponse → Define um modelo padronizado para mensagens de erro na API.
    ✅ Uso de sealed → Impede que a classe seja herdada, garantindo imutabilidade do modelo.
    ✅ Uso de [JsonConstructor] → Permite que a serialização e desserialização utilizem corretamente o construtor primário.
    ✅ Propriedade Message → Armazena a descrição do erro, garantindo que seja acessível e exibível ao cliente.
    ✅ Método ToString() → Retorna a mensagem diretamente, facilitando logs e rastreamento de erros.
    ✅ Arquitetura baseada em respostas padronizadas → Mantém consistência na API e melhora a comunicação de erros com clientes.
    ✅ Essa abordagem melhora a depuração e torna o sistema mais robusto para lidar com falhas.
    */
}