using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries;

public partial class FluentQuery<TQuery>
{
    /// <summary>
    /// Used to suppress the output from the server when writing data. The resulting response will be empty and indicated by a 204 No Content HTTP status code.
    /// </summary>
    /// <returns>
    /// The query with new added query.
    /// </returns>
    public TQuery Silent()
    {
        object query = new Query(Reference, this, _ => new ValueTask<HttpResponse<string>>(new HttpResponse<string>($"print=silent")));

        return (TQuery)query;
    }
}
