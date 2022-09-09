using RestfulFirebase;
using RestfulFirebase.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ObservableHelpers.ComponentModel;
using System.ComponentModel;
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
