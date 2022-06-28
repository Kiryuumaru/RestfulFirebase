using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.CloudFirestore.Query;

public class CollectionQuery : Query
{
    public string Name { get; }

    public DocumentQuery? Parent { get; }

    public CollectionQuery(RestfulFirebaseApp app, DocumentQuery? parent, string name)
        : base (app)
    {
        Name = name;
        Parent = parent;
    }

    public DocumentQuery Document(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        return new DocumentQuery(App, this, name);
    }

    internal override string BuildUrl()
    {
        var url = BuildUrlSegment();

        if (Parent == null)
        {
            url = string.Format(
                CloudFirestoreApp.CloudFirestoreDocumentsEndpoint,
                App.Config.ProjectId,
                CloudFirestoreApp.DefaultDatabase,
                url);
        }
        else
        {
            string parentUrl = Parent.BuildUrl();
            if (parentUrl != string.Empty && !parentUrl.EndsWith("/"))
            {
                parentUrl += '/';
            }
            url = parentUrl + url;
        }

        return url;
    }

    internal override string BuildUrlSegment()
    {
        return Name;
    }
}
