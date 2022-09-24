using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace RestfulFirebase.Common.Internals;

internal class TypedDocumentFieldPair
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type Type { get; }

    public string DocumentFieldName { get; }

    public TypedDocumentFieldPair(Type type, string documentFieldName)
    {
        Type = type;
        DocumentFieldName = documentFieldName;
    }
}
