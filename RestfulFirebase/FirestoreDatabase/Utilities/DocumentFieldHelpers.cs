using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using RestfulFirebase.Common.Attributes;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using RestfulFirebase.Common.Internals;
using ObservableHelpers.ComponentModel;
using System.Collections;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Abstractions;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.FirestoreDatabase.Utilities;

internal static class DocumentFieldHelpers
{
    internal const string DocumentName = "__name__";
}
