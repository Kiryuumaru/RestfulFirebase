using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides realtime observable model for <see cref="RestfulFirebase.Database.Realtime.RealtimeInstance"/>
    /// </summary>
    public interface IRealtimeModel : IObservable
    {
        /// <summary>
        /// Gets the <see cref="RestfulFirebase.Database.Realtime.RealtimeInstance"/> the model uses.
        /// </summary>
        RealtimeInstance RealtimeInstance { get; }

        /// <summary>
        /// Gets <c>true</c> whether model has realtime instance attached; otherwise, <c>false</c>.
        /// </summary>
        bool HasAttachedRealtime { get; }

        /// <summary>
        /// Event raised on current context if the realtime instance is attached on the model.
        /// </summary>
        event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;

        /// <summary>
        /// Event raised on current context if the realtime instance is detached on the model.
        /// </summary>
        event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;

        /// <summary>
        /// Event raised on current context if the realtime instance encounters an error.
        /// </summary>
        event EventHandler<WireException> WireError;

        /// <summary>
        /// Attaches the realtime instance to the model and detaches the current realtime instance.
        /// </summary>
        /// <param name="realtimeInstance">
        /// 
        /// </param>
        /// <param name="invokeSetFirst">
        /// </param>
        void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst);

        /// <summary>
        /// Detaches the realtime instance from the model, if there's an attached realtime instance.
        /// </summary>
        void DetachRealtime();
    }
}
