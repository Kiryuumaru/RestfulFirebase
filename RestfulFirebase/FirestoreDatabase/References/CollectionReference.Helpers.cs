using RestfulFirebase.Common.Internals;
using RestfulFirebase.FirestoreDatabase.Models;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace RestfulFirebase.FirestoreDatabase.References;

public partial class CollectionReference : Reference
{
#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static CollectionReference? Parse(FirebaseApp app, string? json)
    {
        if (json != null && !string.IsNullOrEmpty(json))
        {
            string[] paths = json.Split('/');
            object currentPath = app.FirestoreDatabase.Collection(paths[5]);

            for (int i = 6; i < paths.Length; i++)
            {
                if (currentPath is CollectionReference colPath)
                {
                    currentPath = colPath.Document(paths[i]);
                }
                else if (currentPath is DocumentReference docPath)
                {
                    currentPath = docPath.Collection(paths[i]);
                }
            }

            if (currentPath is CollectionReference collectionReference)
            {
                return collectionReference;
            }
        }

        return null;
    }

#if NET5_0_OR_GREATER
    [RequiresUnreferencedCode(Message.RequiresUnreferencedCodeMessage)]
#endif
    internal static CollectionReference? Parse(FirebaseApp app, JsonElement jsonElement, JsonSerializerOptions jsonSerializerOptions)
    {
        return Parse(app, jsonElement.Deserialize<string>(jsonSerializerOptions));
    }

    internal string BuildUrl(string projectId, string? postSegment = null)
    {
        return $"{FirestoreDatabaseApi.FirestoreDatabaseV1Endpoint}/{BuildUrlCascade(projectId)}{postSegment}";
    }

    internal string BuildUrlCascade(string projectId)
    {
        var url = Id;

        if (Parent == null)
        {
            url = string.Format(
                FirestoreDatabaseApi.FirestoreDatabaseDocumentsEndpoint,
                projectId,
                $"/{url}");
        }
        else
        {
            string parentUrl = Parent.BuildUrlCascade(projectId);
            if (parentUrl != string.Empty && !parentUrl.EndsWith("/"))
            {
                parentUrl += '/';
            }
            url = parentUrl + url;
        }

        return url;
    }
}
