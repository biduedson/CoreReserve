using System.Reflection;

namespace CoreReserve.Core.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<Type> GetAllTypesOf<TInterface>(this Assembly assembly)
        {
            var isAssignableToTInterface = typeof(TInterface).IsAssignableFrom;
            return [.. assembly
         .GetTypes()
         .Where(type => type.IsClass && !type.IsAbstract && !type.IsInterface && isAssignableToTInterface(type))];
        }
    }
}