namespace Mapstor.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class MapMemberAttribute<TargetType>(string targetMember) : Attribute where TargetType : class
{
    public Type Target { get; private set; } = typeof(TargetType);
    public string TargetMember { get; private set; } = targetMember;
}
