using System;
using System.Diagnostics.CodeAnalysis;

namespace RestfulFirebase.Common.Internals;

internal class TypedModelFieldPair
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Type { get; }

    public string ModelFieldName { get; }

    public TypedModelFieldPair(Type type, string modelFieldName)
    {
        Type = type;
        ModelFieldName = modelFieldName;
    }
}
