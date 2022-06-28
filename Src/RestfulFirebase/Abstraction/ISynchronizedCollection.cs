using System.Collections.Specialized;
using System.ComponentModel;

namespace RestfulFirebase.Abstraction;

/// <summary>
/// Contains bundle declarations for observable operations.
/// </summary>
public interface ISynchronizedCollection :
    INullableObject,
    ISynchronizedObject
{
    /// <summary>
    /// Event raised on the current synchronization context when the collection changes.
    /// </summary>
    event NotifyCollectionChangedEventHandler SynchronizedCollectionChanged;
}
