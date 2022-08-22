using System.Collections.Specialized;
using System.ComponentModel;

namespace RestfulFirebase.Abstraction;

/// <summary>
/// Contains bundle declarations for observable operations.
/// </summary>
public interface ISynchronizedCollection1 :
    INullableObject1,
    ISynchronizedObject1
{
    /// <summary>
    /// Event raised on the current synchronization context when the collection changes.
    /// </summary>
    event NotifyCollectionChangedEventHandler SynchronizedCollectionChanged;
}
