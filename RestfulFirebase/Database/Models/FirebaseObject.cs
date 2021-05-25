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
    public class FirebaseObject : ObservableObject, IRealtimeModel
    {
        #region Properties

        public RealtimeModelWire ModelWire { get; private set; }

        #endregion

        #region Methods

        public void StartRealtime(RealtimeModelWire modelWire, bool invokeSetFirst)
        {
            InitializeProperties(false);

            ModelWire = modelWire;
            ModelWire.Subscribe();

            //ModelWire.SetOnChanges(args =>
            //{
            //    OnChanged(nameof(Property));
            //});

            //if (invokeSetFirst)
            //{
            //    ModelWire.SetBlob(blob);
            //}
            //else
            //{
            //    if (blob != GetBlob())
            //    {
            //        OnChanged(nameof(Property));
            //    }
            //}
        }

        public void StopRealtime()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
