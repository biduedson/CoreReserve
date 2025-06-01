namespace CoreReserve.Application.Abstractions
{
    /// <summary>
    /// Interface de marcação para diferenciar as classes dentro do contexto da aplicação.
    /// Esta interface não contém membros e é utilizada apenas como um identificador.
    /// </summary>
    public interface IApplicationMarker;

    // -----------------------------------------
    // 🔹 EXPLICAÇÃO DO CÓDIGO 🔹
    // -----------------------------------------
    /*
    ✅ Interface IApplicationMarker → Interface de marcação sem métodos ou propriedades. 
    ✅ Propósito → Utilizada para distinguir classes dentro do contexto da aplicação sem forçar implementação de membros. 
    ✅ Uso comum → Pode ser usada para identificação dentro de injeção de dependência, reflection ou organização estrutural. 
    ✅ Essa abordagem é útil para categorização de componentes sem afetar a hierarquia ou o comportamento das classes. 
    */
}