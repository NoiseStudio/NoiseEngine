using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace NoiseEngine.CodeGenerators.Shared;

internal static class SyntaxNodeExtensions {

    public static IEnumerable<SyntaxNode> ParentNodes(this SyntaxNode node) {
        SyntaxNode? n = node;
        while ((n = n!.Parent) is not null)
            yield return n;
    }

    public static SemanticModel GetSemanticModel(this SyntaxNode node, Compilation compilation) {
        return compilation.GetSemanticModel(node.SyntaxTree);
    }

    public static SymbolInfo GetSymbolInfo(this SyntaxNode node, Compilation compilation) {
        return node.GetSemanticModel(compilation).GetSymbolInfo(node);
    }

    public static ISymbol? GetSymbol(this SyntaxNode node, Compilation compilation) {
        return node.GetSymbolInfo(compilation).Symbol;
    }

    public static T GetSymbol<T>(this SyntaxNode node, Compilation compilation) where T : ISymbol {
        return (T)(node.GetSymbol(compilation) ?? throw new NullReferenceException());
    }

}
