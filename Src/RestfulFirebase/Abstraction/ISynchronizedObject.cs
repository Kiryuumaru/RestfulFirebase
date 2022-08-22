using System.ComponentModel;

namespace RestfulFirebase.Abstraction;

/// <summary>
/// Contains bundle declarations for observable operations.
/// </summary>
public interface ISynchronizedObject1 :
    INullableObject1,
    INotifyPropertyChanged
{
    /// <summary>
    /// Event raised on the current synchronization context when a property is changed.
    /// </summary>
    event PropertyChangedEventHandler SynchronizedPropertyChanged;
}
