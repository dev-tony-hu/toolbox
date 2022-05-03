using System.Diagnostics.CodeAnalysis;

public static class ConfigurationBinderFixup
{
    public static T GetFixedValue<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(this IConfiguration configuration,string key)
    {
        var t = Activator.CreateInstance<T>();
        configuration.Bind(key,t);
        return t;
    }
}