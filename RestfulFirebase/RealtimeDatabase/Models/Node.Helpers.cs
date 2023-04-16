//using RestfulFirebase.Common.Attributes;
//using RestfulFirebase.Common.Internals;
//using RestfulFirebase.Common.Utilities;
//using RestfulFirebase.RealtimeDatabase.Utilities;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Linq;
//using System.Reflection;
//using System.Text.Json;

//namespace RestfulFirebase.RealtimeDatabase.Models;

//public partial class Node
//{
//#if NET5_0_OR_GREATER
//    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
//#endif
//    internal void BuildUtf8JsonWriter(FirebaseConfig config, Utf8JsonWriter writer, JsonSerializerOptions? jsonSerializerOptions)
//    {
//        object? obj = GetModel();

//        if (obj == null)
//        {
//            ArgumentException.Throw($"Model is a null reference. Provide a model to build to writer.");
//        }

//        Type objType = obj.GetType();

//        ModelBuilderHelpers.BuildUtf8JsonWriter(config, writer, objType, obj, this, jsonSerializerOptions);
//    }

//    internal virtual object? GetModel()
//    {
//        return null;
//    }

//    internal virtual void SetModel(object? obj)
//    {
//        return;
//    }
//}

//public partial class Node<TModel> : Node
//     where TModel : class
//{
//    internal override object? GetModel()
//    {
//        return Model;
//    }

//    internal override void SetModel(object? obj)
//    {
//        if (obj == null)
//        {
//            Model = null;
//        }
//        else if (obj is TModel typedObj)
//        {
//            Model = typedObj;
//        }
//        else
//        {
//            ArgumentException.Throw($"Mismatch type of {nameof(obj)} and {typeof(TModel)}");
//        }
//    }
//}
