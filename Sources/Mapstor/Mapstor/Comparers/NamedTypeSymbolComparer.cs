using Microsoft.CodeAnalysis;

namespace Mapstor.Comparers;

internal class NamedTypeSymbolComparer : IEqualityComparer<INamedTypeSymbol>
{
    public static readonly NamedTypeSymbolComparer Instance = new NamedTypeSymbolComparer();
    public bool Equals(INamedTypeSymbol x, INamedTypeSymbol y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;

        // Comparamos por el nombre completo incluyendo el namespace
        return x.ToDisplayString() == y.ToDisplayString();
    }

    public int GetHashCode(INamedTypeSymbol obj)
    {
        if (obj is null) return 0;

        // Usamos el string del nombre completo para el HashCode
        return obj.ToDisplayString().GetHashCode();
    }
}
