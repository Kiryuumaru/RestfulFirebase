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
            if (invokeSetFirst)
            {

                ModelWire = modelWire;
                ModelWire.Subscribe();

            }
            else
            {
                ModelWire = modelWire;
                ModelWire.Subscribe();
            }
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
