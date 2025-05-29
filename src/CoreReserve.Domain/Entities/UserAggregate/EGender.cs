using System.Text.Json.Serialization;

namespace CoreReserve.Domain.Entities.UserAggregate
{
    [JsonConverter(typeof(JsonStringEnumConverter<EGender>))]
    public enum EGender
    {
        None = 0,
        Male = 1,
        Female = 2
    }
}