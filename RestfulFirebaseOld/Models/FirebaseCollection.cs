using DisposableHelpers.Attributes;

namespace RestfulFirebase.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable dictionary.
    /// </summary>
    /// <typeparam name="TValue">
    /// The undelying type of the dictionary item value.
    /// </typeparam>
    [Disposable]
    public partial class FirebaseCollection<TValue> : ObservableConcurrentCollection<TValue>, IFirebaseModel
    {

    }
}
