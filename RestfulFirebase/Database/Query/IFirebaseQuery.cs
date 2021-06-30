using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using RestfulFirebase.Extensions;

namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// The base declaration for firebase query operations.
    /// </summary>
    public interface IFirebaseQuery
    {
        /// <summary>
        /// Gets the underlying <see cref="RestfulFirebaseApp"/> the module uses.
        /// </summary>
        RestfulFirebaseApp App { get; }

        /// <summary>
        /// Creates new instance of <see cref="ChildQuery"/> node with the specified child <paramref name="pathFactory"/>.
        /// </summary>
        /// <param name="pathFactory">
        /// The resource name of the node.
        /// </param>
        /// <returns>
        /// The created <see cref="ChildQuery"/> node.
        /// </returns>
        ChildQuery Child(Func<string> pathFactory);

        /// <summary>
        /// Creates new instance of <see cref="ChildQuery"/> node with the specified child <paramref name="path"/>.
        /// </summary>
        /// <param name="path">
        /// The resource name of the node.
        /// </param>
        /// <returns>
        /// The created <see cref="ChildQuery"/> node.
        /// </returns>
        ChildQuery Child(string path);

        /// <summary>
        /// Puts or overrides data at the given location.
        /// </summary>
        /// <param name="jsonData">
        /// The json data to put.
        /// </param>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> for the executed put <see cref="Task"/>.
        /// </param>
        /// <param name="onException">
        /// The callback for failed operations.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the executed put <see cref="Task"/>.
        /// </returns>
        Task Put(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);

        /// <summary>
        /// Puts or overrides data at the given location.
        /// </summary>
        /// <param name="jsonData">
        /// The json data to put.
        /// </param>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> for the executed put <see cref="Task"/>.
        /// </param>
        /// <param name="onException">
        /// The callback for failed operations.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the executed put <see cref="Task"/>.
        /// </returns>
        Task Put(Func<string> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);

        /// <summary>
        /// Patches data at given location instead of overwriting them.
        /// </summary> 
        /// <param name="jsonData">
        /// The json data to patch.
        /// </param>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> for the executed patch <see cref="Task"/>.
        /// </param>
        /// <param name="onException">
        /// The callback for failed operations.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the executed patch <see cref="Task"/>.
        /// </returns>
        Task Patch(string jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);

        /// <summary>
        /// Patches data at given location instead of overwriting them.
        /// </summary> 
        /// <param name="jsonData">
        /// The json data to patch.
        /// </param>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> for the executed patch <see cref="Task"/>.
        /// </param>
        /// <param name="onException">
        /// The callback for failed operations.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the executed patch <see cref="Task"/>.
        /// </returns>
        Task Patch(Func<string> jsonData, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);

        /// <summary>
        /// Fan out given item to multiple locations at once. See https://firebase.googleblog.com/2015/10/client-side-fan-out-for-data-consistency_73.html for details.
        /// </summary>
        /// <param name="jsonData">
        /// The json data to fan out.
        /// </param>
        /// <param name="relativePaths">
        /// Locations where to store the data.
        /// </param>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> for the executed fan out <see cref="Task"/>.
        /// </param>
        /// <param name="onException">
        /// The callback for failed operations.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the executed fan out <see cref="Task"/>.
        /// </returns>
        Task FanOut(string jsonData, string[] relativePaths, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);

        /// <summary>
        /// Fan out given item to multiple locations at once. See https://firebase.googleblog.com/2015/10/client-side-fan-out-for-data-consistency_73.html for details.
        /// </summary>
        /// <param name="jsonData">
        /// The json data to fan out.
        /// </param>
        /// <param name="relativePaths">
        /// Locations where to store the data.
        /// </param>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> for the executed fan out <see cref="Task"/>.
        /// </param>
        /// <param name="onException">
        /// The callback for failed operations.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the executed fan out <see cref="Task"/>.
        /// </returns>
        Task FanOut(Func<string> jsonData, string[] relativePaths, CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);

        /// <summary>
        /// Gets the json data of the given location.
        /// </summary>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> for the executed get <see cref="Task"/>.
        /// </param>
        /// <param name="onException">
        /// The callback for failed operations.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the executed get <see cref="Task"/>.
        /// </returns>
        Task<string> Get(CancellationToken? token = null, Action<RetryExceptionEventArgs> onException = null);
        
        /// <summary>
        /// Creates new instance of <see cref="RealtimeWire"/> at the given query location.
        /// </summary>
        /// <returns>
        /// The created <see cref="RealtimeWire"/> of the query location.
        /// </returns>
        RealtimeWire AsRealtimeWire();

        /// <summary>
        /// Builds the url of the query.
        /// </summary>
        /// <param name="token">
        /// The <see cref="CancellationToken"/> of the created <see cref="Task"/>.
        /// </param>
        /// <returns>
        /// The created <see cref="Task"/> represents the built URL.
        /// </returns>
        Task<string> BuildUrl(CancellationToken? token = null);

        /// <summary>
        /// Gets the absolute path of the query.
        /// </summary>
        /// <returns>
        /// The absolute path of the query.
        /// </returns>
        string GetAbsolutePath();
    }
}
