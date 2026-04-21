namespace Mapstor.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public class MapToAttribute<TargetType> : Attribute where TargetType : class
{
    public Type Target { get; private set; } = typeof(TargetType);
}
