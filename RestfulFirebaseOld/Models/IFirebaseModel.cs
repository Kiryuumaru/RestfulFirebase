using System;
using System.ComponentModel;
using SynchronizationContextHelpers;

namespace RestfulFirebase.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable object.
    /// </summary>
    public interface IFirebaseModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Gets the <see cref="SyncOperation"/> used by this object.
        /// </summary>
        SyncOperation SyncOperation { get; }
    }
}
