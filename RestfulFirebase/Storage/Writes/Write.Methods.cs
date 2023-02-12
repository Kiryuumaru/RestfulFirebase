using RestfulHelpers.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Storage.Writes;

public partial class Write
{
    /// <summary>
    /// Gets the awaiter of the specified upload task.
    /// </summary>
    /// <returns>
    /// The awaiter of the specified upload task.
    /// </returns>
    public TaskAwaiter<HttpResponse<string>> GetAwaiter()
    {
        return uploadTask.GetAwaiter();
    }
}
