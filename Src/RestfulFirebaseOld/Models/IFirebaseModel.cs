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
using RestfulFirebase.Local;
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
