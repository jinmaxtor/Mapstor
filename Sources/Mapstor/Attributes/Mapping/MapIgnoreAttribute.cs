namespace Mapstor.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class MapIgnoreAttribute<TargetType> : Attribute where TargetType : class
{
    public Type Target { get; private set; } = typeof(TargetType);
}
