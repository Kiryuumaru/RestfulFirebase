using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DisposableHelpers.Attributes;
using ObservableConcurrentCollections;
using RestfulFirebase;
using RestfulFirebase.Exceptions;
using RestfulFirebase.RealtimeDatabase.Realtime;

namespace RestfulFirebase.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable dictionary.
    /// </summary>
    /// <typeparam name="TValue">
    /// The undelying type of the dictionary item value.
    /// </typeparam>
    [Disposable]
    public partial class FirebaseDictionary<TValue> : ObservableConcurrentDictionary<string, TValue>, IFirebaseModel
    {

    }
}
