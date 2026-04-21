using Mapstor.Attributes;
using Mapstor.Generator.Comparers;
using Mapstor.Generator.Models;
using Mapstor.Generator.Templates;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;

namespace Mapstor.Generator.Generators;

[Generator]
public class MappingGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //Debugger.Launch();

        var isMapToAttribute = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                typeof(MapToAttribute<>).FullName,
                static (node, cancellationToken) => node is ClassDeclarationSyntax,
                static (sintaxContext, cancelationToken) =>
                {
                    var sourceType = sintaxContext.TargetSymbol as INamedTypeSymbol;

                    if (sourceType == null) return ImmutableArray<MappingInfo>.Empty;

                    var mappings = sintaxContext.Attributes.Select(a =>
                    {
                        var targetType = a.AttributeClass!.TypeArguments[0] as INamedTypeSymbol;
                        return new MappingInfo(sourceType, targetType!);
                    });

                    return mappings.ToImmutableArray();
                })
            .SelectMany((mappings, CancellationToken) => mappings);

        var mapToInfos = isMapToAttribute.Collect();
        //var rootNamespace = context.CompilationProvider.Select((compilation, _) => compilation.AssemblyName);
        //var provider = rootNamespace.Combine(mapToInfos);
        var compilationProvider = context.CompilationProvider;
        var provider = compilationProvider.Combine(mapToInfos);

        context.RegisterSourceOutput(provider, static (context, data) =>
        {
            var (compilation, mappings) = data;
            GenerateMappings(context, compilation, mappings);
        });
    }

    private static void GenerateMappings(SourceProductionContext context, Compilation generatorCompilation, ImmutableArray<MappingInfo> mappings)
    {
        if (mappings.IsDefaultOrEmpty) return;

        var mapIgnoreSymbol = generatorCompilation.GetTypeByMetadataName(typeof(MapIgnoreAttribute<>).FullName);
        var mapMemberSymbol = generatorCompilation.GetTypeByMetadataName(typeof(MapMemberAttribute<>).FullName);

        if (mapIgnoreSymbol == null)
        {
            throw new InvalidOperationException($"No se pudo encontrar el tipo {typeof(MapIgnoreAttribute<>).FullName} en la compilación.");
        }

        if (mapMemberSymbol == null)
        {
            throw new InvalidOperationException($"No se pudo encontrar el tipo {typeof(MapMemberAttribute<>).FullName} en la compilación.");
        }

        var groups = mappings.GroupBy(m => m.Source, NamedTypeSymbolComparer.Instance);

        foreach (var group in groups)
        {
            var source = group.Key;
            var targets = group.Select(m => m.Target).ToList();

            var code = GenerateSourceMappingClass(source, mapIgnoreSymbol, mapMemberSymbol, targets);
            context.AddSource($"{source.Name}.Mappings.g.cs", SourceText.From(code, Encoding.UTF8));
        }
    }

    private static string GenerateTargetsCode(INamedTypeSymbol sourceType, INamedTypeSymbol mapIgnoreType, INamedTypeSymbol mapMemberType, List<INamedTypeSymbol> targets, string targetTypeGeneric)
    {
        var sourceProps = sourceType.GetMembers().OfType<IPropertySymbol>().Where(p => p.CanBeReferencedByName);
        var targetMethodsBuilder = new StringBuilder();

        var symbolComparer = NamedTypeSymbolComparer.Instance;
        var typeComparer = SymbolEqualityComparer.Default;

        var dictionaryEntriesBuilder = new StringBuilder();

        foreach (var targetType in targets)
        {
            var targetProps = targetType.GetMembers().OfType<IPropertySymbol>().Where(p => p.CanBeReferencedByName).ToDictionary(p => p.Name);

            dictionaryEntriesBuilder.AppendLine(MappingTemplate.MappingDictionaryEntry(targetType));

            var assignments = new List<string>();

            foreach (var sourceProp in sourceProps)
            {

                // Verificar MapIgnore para este targetType
                var ignoreAttr = sourceProp.GetAttributes()
                    .Where(a => a.AttributeClass != null
                        && symbolComparer.Equals(a.AttributeClass.OriginalDefinition, mapIgnoreType)
                        && a.AttributeClass.TypeArguments.Length > 0
                        && typeComparer.Equals(a.AttributeClass.TypeArguments[0], targetType)
                    )
                    .FirstOrDefault();

                if (ignoreAttr != null) continue;

                // Buscar MapMember para este targetType
                var mapMemberAttr = sourceProp.GetAttributes()
                    .Where(a => a.AttributeClass != null
                        && symbolComparer.Equals(a.AttributeClass.OriginalDefinition, mapMemberType)
                        && a.AttributeClass.TypeArguments.Length > 0
                        && typeComparer.Equals(a.AttributeClass.TypeArguments[0], targetType)
                    )
                    .FirstOrDefault();

                string targetPropName = sourceProp.Name;
                if (mapMemberAttr != null && mapMemberAttr.ConstructorArguments.Length > 0)
                {
                    targetPropName = mapMemberAttr.ConstructorArguments[0].Value as string ?? sourceProp.Name;
                }

                // Verificar si la propiedad target existe y tipos coinciden
                if (targetProps.TryGetValue(targetPropName, out var targetProp) &&
                    targetProp.Type.Equals(sourceProp.Type, SymbolEqualityComparer.Default))
                {
                    assignments.Add(MappingTemplate.PropertyAssignment(sourceProp.Name, targetPropName));
                }
            }

            var instanceAssignments = string.Join(", ", assignments);
            var targetMethodBlock = MappingTemplate.TargetMethodBlock(sourceType, targetType, instanceAssignments);

            targetMethodsBuilder.AppendLine(targetMethodBlock);
        }

        var mappingBlock = MappingTemplate.MappingBlock(sourceType, dictionaryEntriesBuilder.ToString(), targetMethodsBuilder.ToString());

        return mappingBlock;
    }

    private static string GenerateSourceMappingClass(INamedTypeSymbol sourceType, INamedTypeSymbol mapIgnoreType, INamedTypeSymbol mapMemberType, List<INamedTypeSymbol> targets)
    {
        var sourceNamespace = sourceType.ContainingNamespace;

        var body = GenerateTargetsCode(sourceType, mapIgnoreType, mapMemberType, targets, MappingTemplate.TargetGenericTypeName);

        var code = MappingTemplate.ExtensionMethodMapTo(sourceType, body);

        return code;
    }
}
