using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Plate.ModernSatsuma.Generators;

/// <summary>
/// Incremental source generator that inspects types annotated with
/// <see cref="GraphBuilderAttribute"/> and produces graph construction helpers.
///
/// This is a stub implementation that only emits a placeholder file per
/// [GraphBuilder] class, to ensure the generator is wired correctly.
/// </summary>
[Generator]
public sealed class GraphBuilderGenerator : IIncrementalGenerator
{
    private static readonly DiagnosticDescriptor InvalidGraphTypeDescriptor = new(
        id: "MSGB001",
        title: "GraphBuilder GraphType must implement IBuildableGraph",
        messageFormat: "GraphType '{0}' used on GraphBuilder '{1}' must implement '{2}' and have a public parameterless constructor",
        category: "GraphBuilder",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ArcNameNotParsableDescriptor = new(
        id: "MSGB010",
        title: "GraphBuilder arc method name is not in 'From_to_To' format",
        messageFormat: "Arc method '{0}' on GraphBuilder '{1}' must follow the 'From_to_To' naming convention, such as 'One_to_Two'",
        category: "GraphBuilder",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ArcUnknownNodeDescriptor = new(
        id: "MSGB011",
        title: "GraphBuilder arc references unknown node",
        messageFormat: "Arc method '{0}' on GraphBuilder '{1}' refers to unknown node '{2}' (define a [Node] member with this name)",
        category: "GraphBuilder",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor NodeMemberInvalidTypeDescriptor = new(
        id: "MSGB020",
        title: "GraphBuilder node member must be of type Node",
        messageFormat: "Member '{0}' on GraphBuilder '{1}' marked with [Node] must be of type '{2}'",
        category: "GraphBuilder",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    private static readonly DiagnosticDescriptor ArcMethodInvalidSignatureDescriptor = new(
        id: "MSGB021",
        title: "GraphBuilder arc method has an invalid signature",
        messageFormat: "Arc method '{0}' on GraphBuilder '{1}' must have its first two parameters of type '{2}' and return void",
        category: "GraphBuilder",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var graphBuilderInfos =
            context.SyntaxProvider
                .CreateSyntaxProvider(
                    static (node, _) => IsGraphBuilderCandidate(node),
                    static (ctx, _) => GetGraphBuilderInfo(ctx))
                .Where(static info => info is not null)
                .Collect();

        var compilationAndBuilders = context.CompilationProvider.Combine(graphBuilderInfos);

        context.RegisterSourceOutput(compilationAndBuilders, static (spc, pair) =>
        {
            var compilation = pair.Left;
            var infos = pair.Right;

            foreach (var info in infos)
            {
                if (info is null)
                {
                    continue;
                }

                var (symbol, attribute) = info.Value;
                EmitGraphBuilder(spc, compilation, symbol, attribute);
            }
        });
    }

    private static bool IsGraphBuilderCandidate(SyntaxNode node)
        => node is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static (INamedTypeSymbol Symbol, AttributeData Attribute)? GetGraphBuilderInfo(GeneratorSyntaxContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDecl)
        {
            return null;
        }

        var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
        if (symbol is null)
        {
            return null;
        }

        foreach (var attribute in symbol.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null)
            {
                continue;
            }

            if (attributeClass.Name is "GraphBuilderAttribute" or "GraphBuilder")
            {
                return (symbol, attribute);
            }
        }

        return null;
    }

    private static void EmitGraphBuilder(SourceProductionContext context, Compilation compilation, INamedTypeSymbol builderSymbol, AttributeData graphBuilderAttribute)
    {
        if (builderSymbol.ContainingType is not null)
        {
            return;
        }

        var defaultGraphTypeSymbol = compilation.GetTypeByMetadataName("Plate.ModernSatsuma.CustomGraph");
        var buildableGraphSymbol = compilation.GetTypeByMetadataName("Plate.ModernSatsuma.IBuildableGraph");
        if (defaultGraphTypeSymbol is null || buildableGraphSymbol is null)
        {
            return;
        }

        var graphTypeSymbol = ResolveGraphTypeSymbol(context, builderSymbol, graphBuilderAttribute, defaultGraphTypeSymbol, buildableGraphSymbol);
        if (graphTypeSymbol is null)
        {
            return;
        }

        var defaultDirectednessLiteral = GetDefaultDirectednessLiteral(graphBuilderAttribute);

        var nodeTypeSymbol = compilation.GetTypeByMetadataName("Plate.ModernSatsuma.Node");

        var nodeMembers = new List<ISymbol>();
        var arcMethods = new List<(IMethodSymbol Method, AttributeData Attribute)>();

        foreach (var member in builderSymbol.GetMembers())
        {
            if (member is IPropertySymbol or IFieldSymbol)
            {
                if (HasAttribute(member, "NodeAttribute", "Node"))
                {
                    if (nodeTypeSymbol is INamedTypeSymbol nodeNamedType && !IsNodeTypedMember(member, nodeNamedType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(
                            NodeMemberInvalidTypeDescriptor,
                            member.Locations.Length > 0
                                ? member.Locations[0]
                                : (builderSymbol.Locations.Length > 0 ? builderSymbol.Locations[0] : Location.None),
                            member.Name,
                            builderSymbol.Name,
                            nodeNamedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                        continue;
                    }

                    nodeMembers.Add(member);
                }
            }
            else if (member is IMethodSymbol method)
            {
                foreach (var attribute in method.GetAttributes())
                {
                    var attributeClass = attribute.AttributeClass;
                    if (attributeClass is null)
                    {
                        continue;
                    }

                    if (attributeClass.Name == "ArcAttribute" || attributeClass.Name == "Arc")
                    {
                        if (nodeTypeSymbol is INamedTypeSymbol nodeNamedType && !IsValidArcMethodSignature(method, nodeNamedType))
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                ArcMethodInvalidSignatureDescriptor,
                                method.Locations.Length > 0
                                    ? method.Locations[0]
                                    : (builderSymbol.Locations.Length > 0 ? builderSymbol.Locations[0] : Location.None),
                                method.Name,
                                builderSymbol.Name,
                                nodeNamedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
                            break;
                        }

                        arcMethods.Add((method, attribute));
                        break;
                    }
                }
            }
        }

        if (nodeMembers.Count == 0)
        {
            return;
        }

        var nodesByName = new Dictionary<string, ISymbol>(StringComparer.Ordinal);
        foreach (var nodeMember in nodeMembers)
        {
            if (!nodesByName.ContainsKey(nodeMember.Name))
            {
                nodesByName[nodeMember.Name] = nodeMember;
            }

            var logicalName = GetNodeLogicalName(nodeMember);
            if (!string.Equals(logicalName, nodeMember.Name, StringComparison.Ordinal)
                && !nodesByName.ContainsKey(logicalName))
            {
                nodesByName[logicalName] = nodeMember;
            }
        }

        var namespaceName = builderSymbol.ContainingNamespace?.IsGlobalNamespace == false
            ? builderSymbol.ContainingNamespace.ToDisplayString()
            : null;

        var graphTypeName = graphTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var accessibility = GetAccessibilityModifier(builderSymbol.DeclaredAccessibility);

        var hintName = builderSymbol.Name + ".GraphBuilder.g.cs";
        var builder = new StringBuilder();

        if (!string.IsNullOrEmpty(namespaceName))
        {
            builder.Append("namespace ").Append(namespaceName).AppendLine(";");
            builder.AppendLine();
        }

        if (!string.IsNullOrEmpty(accessibility))
        {
            builder.Append(accessibility).Append(' ');
        }

        builder.Append("partial class ").Append(builderSymbol.Name).AppendLine();
        builder.AppendLine("{");

        builder.AppendLine("    private readonly global::System.Collections.Generic.Dictionary<global::Plate.ModernSatsuma.Arc, double> _graphBuilderArcCosts = new global::System.Collections.Generic.Dictionary<global::Plate.ModernSatsuma.Arc, double>();");
        builder.AppendLine("    private readonly global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.Dictionary<global::Plate.ModernSatsuma.Arc, double>> _graphBuilderNamedArcCosts = new global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.Dictionary<global::Plate.ModernSatsuma.Arc, double>>();");
        builder.AppendLine("    private readonly global::System.Collections.Generic.Dictionary<global::Plate.ModernSatsuma.Arc, string> _graphBuilderArcTags = new global::System.Collections.Generic.Dictionary<global::Plate.ModernSatsuma.Arc, string>();");
        builder.AppendLine();
        builder.AppendLine("    public global::System.Func<global::Plate.ModernSatsuma.Arc, double> CreateCostFunction(double defaultCost = 1.0)");
        builder.AppendLine("    {");
        builder.AppendLine("        return arc => _graphBuilderArcCosts.TryGetValue(arc, out var cost) ? cost : defaultCost;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public global::System.Func<global::Plate.ModernSatsuma.Arc, double> CreateCostFunction(string weightName, double defaultCost = 1.0)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (weightName is null) throw new global::System.ArgumentNullException(nameof(weightName));");
        builder.AppendLine("        return arc => _graphBuilderNamedArcCosts.TryGetValue(weightName, out var weights) && weights.TryGetValue(arc, out var cost) ? cost : defaultCost;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public global::System.Func<global::Plate.ModernSatsuma.Arc, string?> CreateTagLookup()");
        builder.AppendLine("    {");
        builder.AppendLine("        return arc => _graphBuilderArcTags.TryGetValue(arc, out var tag) ? tag : null;");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.Append("    public ").Append(graphTypeName).Append(" BuildGraph()").AppendLine();
        builder.AppendLine("    {");
        builder.Append("        var graph = new ").Append(graphTypeName).AppendLine("();");
        builder.AppendLine("        _graphBuilderArcCosts.Clear();");
        builder.AppendLine("        _graphBuilderNamedArcCosts.Clear();");
        builder.AppendLine("        _graphBuilderArcTags.Clear();");
        builder.AppendLine("        global::Plate.ModernSatsuma.Arc arc;");

        foreach (var nodeMember in nodeMembers)
        {
            builder.Append("        ").Append(nodeMember.Name).AppendLine(" = graph.AddNode();");
        }

        foreach (var (method, arcAttribute) in arcMethods)
        {
            var (fromName, toName) = ParseArcName(method.Name);
            if (fromName is null || toName is null)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ArcNameNotParsableDescriptor,
                    method.Locations.Length > 0 ? method.Locations[0] : Location.None,
                    method.Name,
                    builderSymbol.Name));
                continue;
            }

            if (!nodesByName.ContainsKey(fromName))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ArcUnknownNodeDescriptor,
                    method.Locations.Length > 0 ? method.Locations[0] : Location.None,
                    method.Name,
                    builderSymbol.Name,
                    fromName));
                continue;
            }

            if (!nodesByName.ContainsKey(toName))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    ArcUnknownNodeDescriptor,
                    method.Locations.Length > 0 ? method.Locations[0] : Location.None,
                    method.Name,
                    builderSymbol.Name,
                    toName));
                continue;
            }

            var directednessLiteral = GetDirectednessLiteral(arcAttribute, defaultDirectednessLiteral);
            var costLiteral = GetArcCostLiteral(arcAttribute);
            var tagLiteral = GetArcTagLiteral(arcAttribute);
            var weightNameLiteral = GetArcWeightNameLiteral(arcAttribute);

            var fromNodeSymbol = nodesByName[fromName];
            var toNodeSymbol = nodesByName[toName];

            if (costLiteral is not null || tagLiteral is not null)
            {
                builder.Append("        arc = graph.AddArc(")
                    .Append(fromNodeSymbol.Name)
                    .Append(", ")
                    .Append(toNodeSymbol.Name)
                    .Append(", ")
                    .Append(directednessLiteral)
                    .Append(");")
                    .AppendLine();

                if (costLiteral is not null)
                {
                    builder.Append("        _graphBuilderArcCosts[arc] = ")
                        .Append(costLiteral)
                        .AppendLine(";");

                    if (weightNameLiteral is not null)
                    {
                        builder.Append("        if (!_graphBuilderNamedArcCosts.TryGetValue(")
                            .Append(weightNameLiteral)
                            .Append(", out var weights))")
                            .AppendLine();
                        builder.AppendLine("        {");
                        builder.Append("            weights = _graphBuilderNamedArcCosts[")
                            .Append(weightNameLiteral)
                            .Append("] = new global::System.Collections.Generic.Dictionary<global::Plate.ModernSatsuma.Arc, double>();")
                            .AppendLine();
                        builder.AppendLine("        }");
                        builder.Append("        weights[arc] = ")
                            .Append(costLiteral)
                            .AppendLine(";");
                    }
                }

                if (tagLiteral is not null)
                {
                    builder.Append("        _graphBuilderArcTags[arc] = ")
                        .Append(tagLiteral)
                        .AppendLine(";");
                }
            }
            else
            {
                builder.Append("        graph.AddArc(")
                    .Append(fromNodeSymbol.Name)
                    .Append(", ")
                    .Append(toNodeSymbol.Name)
                    .Append(", ")
                    .Append(directednessLiteral)
                    .Append(");")
                    .AppendLine();
            }
        }

        builder.AppendLine("        return graph;");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource(hintName, builder.ToString());
    }

    private static bool HasAttribute(ISymbol symbol, string attributeName, string shortName)
    {
        foreach (var attribute in symbol.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null)
            {
                continue;
            }

            if (attributeClass.Name == attributeName || attributeClass.Name == shortName)
            {
                return true;
            }
        }

        return false;
    }

    private static (string? From, string? To) ParseArcName(string methodName)
    {
        var index = methodName.IndexOf("_to_", StringComparison.OrdinalIgnoreCase);
        if (index <= 0 || index + 4 >= methodName.Length)
        {
            return (null, null);
        }

        var from = methodName.Substring(0, index);
        var to = methodName.Substring(index + 4);
        return (from, to);
    }

    private static INamedTypeSymbol? ResolveGraphTypeSymbol(
        SourceProductionContext context,
        INamedTypeSymbol builderSymbol,
        AttributeData graphBuilderAttribute,
        INamedTypeSymbol defaultGraphType,
        INamedTypeSymbol buildableGraphSymbol)
    {
        ITypeSymbol? selectedType = defaultGraphType;

        foreach (var argument in graphBuilderAttribute.NamedArguments)
        {
            if (argument.Key == nameof(GraphBuilderAttribute.GraphType) && argument.Value.Value is ITypeSymbol typeSymbol)
            {
                selectedType = typeSymbol;
                break;
            }
        }

        if (selectedType is not INamedTypeSymbol namedType)
        {
            return defaultGraphType;
        }

        bool implementsBuildable =
            SymbolEqualityComparer.Default.Equals(namedType, buildableGraphSymbol) ||
            ImplementsInterface(namedType, buildableGraphSymbol);

        bool hasPublicParameterlessCtor = false;
        foreach (var ctor in namedType.InstanceConstructors)
        {
            if (ctor.Parameters.Length == 0 && ctor.DeclaredAccessibility == Accessibility.Public)
            {
                hasPublicParameterlessCtor = true;
                break;
            }
        }

        if (!implementsBuildable || !hasPublicParameterlessCtor)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                InvalidGraphTypeDescriptor,
                builderSymbol.Locations.Length > 0 ? builderSymbol.Locations[0] : Location.None,
                namedType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                builderSymbol.Name,
                buildableGraphSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
            return null;
        }

        return namedType;
    }

    private static bool ImplementsInterface(INamedTypeSymbol type, INamedTypeSymbol interfaceType)
    {
        foreach (var iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface, interfaceType))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsNodeTypedMember(ISymbol member, INamedTypeSymbol nodeType)
    {
        ITypeSymbol? memberType = null;

        if (member is IPropertySymbol property)
        {
            memberType = property.Type;
        }
        else if (member is IFieldSymbol field)
        {
            memberType = field.Type;
        }

        return memberType is not null && SymbolEqualityComparer.Default.Equals(memberType, nodeType);
    }

    private static bool IsValidArcMethodSignature(IMethodSymbol method, INamedTypeSymbol nodeType)
    {
        if (!method.ReturnsVoid)
        {
            return false;
        }

        if (method.Parameters.Length < 2)
        {
            return false;
        }

        var first = method.Parameters[0].Type;
        var second = method.Parameters[1].Type;

        return SymbolEqualityComparer.Default.Equals(first, nodeType)
            && SymbolEqualityComparer.Default.Equals(second, nodeType);
    }

    private static string GetNodeLogicalName(ISymbol nodeMember)
    {
        foreach (var attribute in nodeMember.GetAttributes())
        {
            var attributeClass = attribute.AttributeClass;
            if (attributeClass is null)
            {
                continue;
            }

            if (attributeClass.Name == "NodeAttribute" || attributeClass.Name == "Node")
            {
                foreach (var argument in attribute.NamedArguments)
                {
                    if (argument.Key == nameof(NodeAttribute.Name))
                    {
                        if (argument.Value.Value is string name && !string.IsNullOrEmpty(name))
                        {
                            return name;
                        }
                    }
                }
            }
        }

        return nodeMember.Name;
    }

    private static string GetDefaultDirectednessLiteral(AttributeData graphBuilderAttribute)
    {
        foreach (var argument in graphBuilderAttribute.NamedArguments)
        {
            if (argument.Key == nameof(GraphBuilderAttribute.DefaultDirectedness))
            {
                var value = argument.Value.Value;
                if (value is int intValue)
                {
                    // Directedness enum: Directed = 0, Undirected = 1
                    return intValue switch
                    {
                        0 => "global::Plate.ModernSatsuma.Directedness.Directed",
                        1 => "global::Plate.ModernSatsuma.Directedness.Undirected",
                        _ => "global::Plate.ModernSatsuma.Directedness.Directed"
                    };
                }
            }
        }

        return "global::Plate.ModernSatsuma.Directedness.Directed";
    }

    private static string? GetArcCostLiteral(AttributeData arcAttribute)
    {
        foreach (var argument in arcAttribute.NamedArguments)
        {
            if (argument.Key == nameof(ArcAttribute.Cost) && argument.Value.Value is double cost)
            {
                return cost.ToString("R", CultureInfo.InvariantCulture);
            }
        }

        return null;
    }

    private static string? GetArcTagLiteral(AttributeData arcAttribute)
    {
        foreach (var argument in arcAttribute.NamedArguments)
        {
            if (argument.Key == nameof(ArcAttribute.Tag) && argument.Value.Value is string tag && !string.IsNullOrEmpty(tag))
            {
                return ToStringLiteral(tag);
            }
        }

        return null;
    }

    private static string? GetArcWeightNameLiteral(AttributeData arcAttribute)
    {
        foreach (var argument in arcAttribute.NamedArguments)
        {
            if (argument.Key == nameof(ArcAttribute.WeightName) && argument.Value.Value is string name && !string.IsNullOrEmpty(name))
            {
                return ToStringLiteral(name);
            }
        }

        return null;
    }

    private static string ToStringLiteral(string value)
    {
        var builder = new StringBuilder();
        builder.Append('"');

        foreach (var ch in value)
        {
            switch (ch)
            {
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    builder.Append(ch);
                    break;
            }
        }

        builder.Append('"');
        return builder.ToString();
    }

    private static string GetDirectednessLiteral(AttributeData arcAttribute, string defaultDirectednessLiteral)
    {
        foreach (var argument in arcAttribute.NamedArguments)
        {
            if (argument.Key == nameof(ArcAttribute.Directedness))
            {
                var value = argument.Value.Value;
                if (value is int intValue)
                {
                    // Directedness enum: Directed = 0, Undirected = 1
                    return intValue switch
                    {
                        0 => "global::Plate.ModernSatsuma.Directedness.Directed",
                        1 => "global::Plate.ModernSatsuma.Directedness.Undirected",
                        _ => defaultDirectednessLiteral
                    };
                }
            }
        }

        return defaultDirectednessLiteral;
    }

    private static string GetAccessibilityModifier(Accessibility accessibility)
        => accessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.Private => "private",
            _ => string.Empty
        };
}
