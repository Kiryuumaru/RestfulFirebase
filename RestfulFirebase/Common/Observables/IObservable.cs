﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RestfulFirebase.Common.Observables
{
    public interface IObservable : INotifyPropertyChanged
    {
        void OnError(Exception exception, bool defaultIgnoreAndContinue = true);
        void OnError(ContinueExceptionEventArgs args);
    }
}
