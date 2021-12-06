using System;
using System.Linq;

namespace Core.Reflection;

public static class TypeProvider
{
    public static Type? GetFirstMatchingTypeFromCurrentDomainAssembly(string typeName)
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes().Where(x => x.FullName == typeName || x.Name == typeName))
            .FirstOrDefault();
    }
}