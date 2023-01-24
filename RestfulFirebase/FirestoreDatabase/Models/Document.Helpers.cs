using RestfulFirebase.Common.Attributes;
using RestfulFirebase.Common.Internals;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.Models;

public partial class Document
{
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
    internal void BuildUtf8JsonWriter(FirebaseConfig config, Utf8JsonWriter writer, JsonSerializerOptions? jsonSerializerOptions)
    {
        object? obj = GetModel();

        if (obj == null)
        {
            ArgumentException.Throw($"Model is a null reference. Provide a model to build to writer.");
        }

        Type objType = obj.GetType();

        ModelBuilderHelpers.BuildUtf8JsonWriter(config, writer, objType, obj, this, jsonSerializerOptions);
    }

    internal virtual object? GetModel()
    {
        return null;
    }

    internal virtual void SetModel(object? obj)
    {
        return;
    }
}

public partial class Document<TModel> : Document
     where TModel : class
{
    internal override object? GetModel()
    {
        return Model;
    }

    internal override void SetModel(object? obj)
    {
        if (obj == null)
        {
            Model = null;
        }
        else if (obj is TModel typedObj)
        {
            Model = typedObj;
        }
        else
        {
            ArgumentException.Throw($"Mismatch type of {nameof(obj)} and {typeof(TModel)}");
        }
    }
}
