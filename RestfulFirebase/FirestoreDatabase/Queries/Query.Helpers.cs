using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Queries;

public abstract partial class BaseQuery<TQuery>
{ 
    internal string GetDocumentPath(string[] namePath)
    {
        var documentFieldPath = DocumentFieldHelpers.GetDocumentFieldPath(ModelType, namePath, App.FirestoreDatabase.ConfigureJsonSerializerOption());

        return string.Join(".", documentFieldPath.Select(i => i.DocumentFieldName));
    }
}
