using ObservableHelpers.ComponentModel;
using DisposableHelpers.Attributes;
using SynchronizationContextHelpers;

namespace RestfulFirebase.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable object.
    /// </summary>
    [Disposable]
    public partial class FirebaseObject : ObservableObject, IFirebaseModel
    {
        /// <inheritdoc/>
        public SyncOperation SyncOperation { get; } = new();
    }
}
