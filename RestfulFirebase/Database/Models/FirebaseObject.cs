using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObject : ObservableObject, IRealtimeModelProxy
    {
        #region Properties

        private RealtimeModelWire modelWire;

        #endregion

        #region Methods

        public void Start()
        {
            //if (!modelWire.Wire.Started) modelWire.Wire.Start();
            modelWire?.Subscribe();
        }

        public void Stop()
        {
            modelWire?.Unsubscribe();
        }

        public void Dispose()
        {
            Stop();
        }

        void IRealtimeModelProxy.StartRealtime(RealtimeModelWire modelWire, bool invokeSetFirst)
        {
            this.modelWire = modelWire;
            modelWire.SetOnSubscribed(delegate
            {
                //modelWire.SetOnChanges(args =>
                //{
                //    OnChanged(nameof(Property));
                //});

                //var blob = GetBlob(UnwiredBlobTag);

                //if (invokeSetFirst)
                //{
                //    this.modelWire.SetBlob(blob);
                //}
                //else
                //{
                //    if (blob != GetBlob())
                //    {
                //        OnChanged(nameof(Property));
                //    }
                //}
            });
        }

        #endregion
    }
}
