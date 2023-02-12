using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using RestfulFirebase.Storage.Exceptions;
using RestfulFirebase.Common.Abstractions;
using RestfulHelpers.Common;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using TransactionHelpers;
using RestfulHelpers;
using RestfulFirebase.Common.Utilities;

namespace RestfulFirebase.Storage.References;

public partial class Reference
{
    private string GetTargetUrl()
    {
        return $"{StorageApi.FirebaseStorageEndpoint}{Bucket.Name}/o?name={GetEscapedPath()}";
    }

    private string GetDownloadUrl()
    {
        return $"{StorageApi.FirebaseStorageEndpoint}{Bucket.Name}/o/{GetEscapedPath()}";
    }

    private string GetFullDownloadUrl()
    {
        return GetDownloadUrl() + "?alt=media&token=";
    }

    private string GetEscapedPath()
    {
        return Uri.EscapeDataString(string.Join("/", Path));
    }
}
