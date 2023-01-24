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
    /// This is an advanced feature, designed to help you work with large datasets without needing to download everything. Set this to true to limit the depth of the data returned at a location. If the data at the location is a JSON primitive (string, number or boolean), its value will simply be returned. If the data snapshot at the location is a JSON object, the values for each key will be truncated to true.
    /// </summary>
    /// <returns>
    /// The query with new added query.
    /// </returns>
    public TQuery Shallow()
    {
        object query = new Query(Reference, this, _ => new ValueTask<HttpResponse<string>>(new HttpResponse<string>($"shallow=true")));

        return (TQuery)query;
    }
}
