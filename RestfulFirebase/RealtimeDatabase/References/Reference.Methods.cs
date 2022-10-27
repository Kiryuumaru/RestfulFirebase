using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using RestfulFirebase.RealtimeDatabase.Queries;
using RestfulFirebase.Common.Http;

namespace RestfulFirebase.RealtimeDatabase.References;

public partial class Reference
{
    /// <summary>
    /// Creates new instance of <see cref="Queries.Query"/>.
    /// </summary>
    /// <returns>
    /// The created <see cref="Reference"/> node.
    /// </returns>
    public OrderedQuery Query()
    {
        return new OrderedQuery(this, null, _ => new ValueTask<HttpResponse<string>>(new HttpResponse<string>()));
    }

    /// <summary>
    /// Creates new instance of <see cref="Reference"/> node with the specified <paramref name="path"/>.
    /// </summary>
    /// <param name="path">
    /// The path of the node.
    /// </param>
    /// <returns>
    /// The created <see cref="Reference"/> node.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="path"/> is empty or has forbidden character.
    /// </exception>
    public Reference Child(string path)
    {
        return Create(RealtimeDatabase, this, path);
    }
}
