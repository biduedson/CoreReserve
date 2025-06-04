namespace CoreReserve.Query.Abstractions
{
    /// <summary>
    /// Marcador para identificar classes relacionadas a consultas.
    /// Utilizado para localizar e registrar automaticamente serviços de consulta na aplicação.
    /// </summary>
    public interface IQueryMarker;

}
/* ✅ Interface IQueryMarker → Não contém métodos ou propriedades, servindo apenas como identificador. 
 * ✅ Propósito → Usado para localizar e registrar automaticamente serviços relacionados a consultas. 
 * ✅ Uso no projeto → Essa interface geralmente é usada em conjunto com reflexão (Assembly.GetAssembly(typeof(IQueryMarker)))
 * para identificar componentes específicos na camada de Query. ✅ Arquitetura baseada em CQRS → Separa operações de leitura e escrita, 
 * garantindo escalabilidade e organização.*/
