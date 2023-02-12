using RestfulFirebase.Storage.Buckets;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Storage;

public partial class StorageApi
{
    /// <summary>
    /// Creates new instance of <see cref="Buckets.Bucket"/> reference.
    /// </summary>
    /// <param name="bucket">
    /// The storage bucket. Set to <c>null</c> if the instance will use the default firebase storage (i.e., "projectid.appspot.com").
    /// </param>
    /// <returns>
    /// The instance of <see cref="Buckets.Bucket"/> reference.
    /// </returns>
    public Bucket Bucket(string? bucket = default)
    {
        if (bucket == null || string.IsNullOrEmpty(bucket))
        {
            bucket = $"{App.Config.ProjectId}.appspot.com";
        }

        return new Bucket(App, bucket);
    }
}