using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Extensions;
using RestfulFirebase.Serializers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    /// <summary>
    /// Provides an observable model for the firebase realtime instance for an observable property.
    /// </summary>
    public class FirebaseProperty : ObservableProperty, IRealtimeModel
    {
        #region Properties

        /// <inheritdoc/>
        public RealtimeInstance RealtimeInstance { get; private set; }

        /// <inheritdoc/>
        public bool HasAttachedRealtime { get => !(RealtimeInstance?.IsDisposed ?? true); }

        /// <inheritdoc/>
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;

        /// <inheritdoc/>
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;

        /// <inheritdoc/>
        public event EventHandler<WireExceptionEventArgs> WireError;

        private SemaphoreSlim attachLock = new SemaphoreSlim(1, 1);

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseProperty"/> class.
        /// </summary>
        public FirebaseProperty()
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public async void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (IsDisposed)
            {
                return;
            }

            var obj = GetObject();

            try
            {
                await attachLock.WaitAsync().ConfigureAwait(false);

                Subscribe(realtimeInstance);

                if (obj is IRealtimeModel model)
                {
                    model.AttachRealtime(realtimeInstance, invokeSetFirst);
                }
                else
                {
                    string blob = null;
                    if (obj is string objBlob)
                    {
                        blob = objBlob;
                    }
                    else if (obj is null)
                    {
                        blob = null;
                    }
                    else
                    {
                        throw new SerializerNotSupportedException(obj.GetType());
                    }

                    if (invokeSetFirst)
                    {
                        RealtimeInstance.SetBlob(blob);
                    }
                    else
                    {
                        SetObject(RealtimeInstance.GetBlob());
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                attachLock.Release();
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        /// <inheritdoc/>
        public void DetachRealtime()
        {
            if (IsDisposed || !HasAttachedRealtime)
            {
                return;
            }

            if (GetObject() is IRealtimeModel model)
            {
                model.DetachRealtime();
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
        }

        /// <inheritdoc/>
        public void LoadFromSerializedValue(string serialized)
        {
            if (IsDisposed)
            {
                return;
            }

            if (GetObject() is IRealtimeModel model)
            {
                model.LoadFromSerializedValue(serialized);
            }
            else
            {
                if (SetObject(serialized))
                {
                    if (HasAttachedRealtime)
                    {
                        RealtimeInstance.SetBlob(serialized);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void LoadFromSerializedValue(string serialized, int[] encryptionPattern)
        {
            if (IsDisposed)
            {
                return;
            }

            var decrypted = Utils.DecryptString(serialized, encryptionPattern);
            LoadFromSerializedValue(decrypted);
        }

        /// <inheritdoc/>
        public string GenerateSerializedValue()
        {
            if (IsDisposed)
            {
                return default;
            }

            object obj = GetObject();
            if (obj is IRealtimeModel model)
            {
                return model.GenerateSerializedValue();
            }
            else if (obj is string value)
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public string GenerateSerializedValue(int[] encryptionPattern)
        {
            if (IsDisposed)
            {
                return default;
            }

            string serialized = GenerateSerializedValue();
            if (serialized != null)
            {
                return Utils.EncryptString(serialized, encryptionPattern);
            }
            else
            {
                return null;
            }
        }

        /// <inheritdoc/>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        public override bool SetValue<T>(T value)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (value is IRealtimeModel model)
                {
                    if (HasAttachedRealtime)
                    {
                        model.AttachRealtime(RealtimeInstance, true);
                    }
                }

                return SetObject(value);
            }
            else
            {
                if (!Serializer.CanSerialize<T>())
                {
                    throw new SerializerNotSupportedException(typeof(T));
                }

                var blob = Serializer.Serialize(value);

                if (SetObject(blob))
                {
                    if (HasAttachedRealtime)
                    {
                        RealtimeInstance.SetBlob(blob);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        public override T GetValue<T>(T defaultValue = default)
        {
            if (IsDisposed)
            {
                return defaultValue;
            }

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                return base.GetValue(defaultValue);
            }
            else
            {
                if (!Serializer.CanSerialize<T>()) throw new SerializerNotSupportedException(typeof(T));

                var obj = GetObject();

                string blob = null;
                if (obj is string objBlob)
                {
                    blob = objBlob;
                }
                else if (obj is null)
                {
                    blob = null;
                }
                else
                {
                    throw new SerializerNotSupportedException(obj.GetType());
                }

                return Serializer.Deserialize<T>(blob, defaultValue);
            }
        }

        /// <inheritdoc/>
        public override bool SetNull()
        {
            if (IsDisposed)
            {
                return false;
            }

            if (GetObject() is IRealtimeModel)
            {
                return base.SetNull();
            }
            else
            {
                if (base.SetNull())
                {
                    if (HasAttachedRealtime)
                    {
                        RealtimeInstance.SetBlob(null);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var obj = GetObject();

                DetachRealtime();
                SetObject(null);

                if (obj is IDisposable model)
                {
                    model.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Invokes <see cref="RealtimeAttached"/> event on the current context.
        /// </summary>
        /// <param name="args">
        /// The event arguments for the event to invoke.
        /// </param>
        protected virtual void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            ContextSend(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        /// <summary>
        /// Invokes <see cref="RealtimeDetached"/> event on the current context.
        /// </summary>
        /// <param name="args">
        /// The event arguments for the event to invoke.
        /// </param>
        protected virtual void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            ContextSend(delegate
            {
                RealtimeDetached?.Invoke(this, args);
            });
        }

        /// <summary>
        /// Invokes <see cref="WireError"/> event on the current context.
        /// </summary>
        /// <param name="args">
        /// The event arguments for the event to invoke.
        /// </param>
        protected virtual void OnWireError(WireExceptionEventArgs args)
        {
            ContextPost(delegate
            {
                WireError?.Invoke(this, args);
            });
        }

        private void Subscribe(RealtimeInstance realtimeInstance)
        {
            if (IsDisposed)
            {
                return;
            }

            if (HasAttachedRealtime)
            {
                Unsubscribe();
            }

            RealtimeInstance = realtimeInstance;

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
                RealtimeInstance.Disposing += RealtimeInstance_Disposing;
            }
        }

        private void Unsubscribe()
        {
            if (IsDisposed)
            {
                return;
            }

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
                RealtimeInstance.Disposing -= RealtimeInstance_Disposing;
            }

            RealtimeInstance = null;
        }

        private async void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            var path = Utils.UrlSeparate(e.Path);

            if (path.Length == 0)
            {
                try
                {
                    await attachLock.WaitAsync().ConfigureAwait(false);
                    if (!(GetObject() is IRealtimeModel))
                    {
                        SetObject(RealtimeInstance.GetBlob());
                    }
                }
                catch
                {
                    throw;
                }
                finally
                {
                    attachLock.Release();
                }
            }
        }

        private void RealtimeInstance_Error(object sender, WireExceptionEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            OnWireError(e);
        }

        private void RealtimeInstance_Disposing(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            DetachRealtime();
        }

        #endregion
    }

    /// <inheritdoc/>
    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Properties

        /// <summary>
        /// Gets the value of the property.
        /// </summary>
        public T Value
        {
            get => base.GetValue<T>(default);
            set => base.SetValue(value);
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="FirebaseProperty{T}"/> class.
        /// </summary>
        /// <exception cref="SerializerNotSupportedException">
        /// Occurs when the object has no supported serializer.
        /// </exception>
        public FirebaseProperty()
        {
            if (!Serializer.CanSerialize<T>()) throw new SerializerNotSupportedException(typeof(T));
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (args.PropertyName == nameof(Property))
            {
                base.OnPropertyChanged(new PropertyChangedEventArgs(nameof(Value)));
            }
        }

        #endregion
    }
}
