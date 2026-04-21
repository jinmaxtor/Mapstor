using Microsoft.CodeAnalysis;

namespace Mapstor.Models;

public sealed class MappingInfo(INamedTypeSymbol Source, INamedTypeSymbol Target)
{
    public string SourceNamespace => Source.ContainingNamespace.ToDisplayString();
    public string SourceName => Source.Name;
    public INamedTypeSymbol Source { get; set; } = Source;

    public string TargetNamespace => Target.ContainingNamespace.ToDisplayString();
    public string TargetName => Target.Name;
    public INamedTypeSymbol Target { get; set; } = Target;
}
