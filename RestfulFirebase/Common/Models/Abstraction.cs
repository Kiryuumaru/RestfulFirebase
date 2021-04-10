using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public interface IAttributed
    {
        AttributeHolder Holder { get; }
    }

    public interface IObservableAttributed : IAttributed, INotifyPropertyChanged
    {
        void OnError(Exception exception, bool defaultIgnoreAndContinue = true);
        void OnError(ContinueExceptionEventArgs args);
    }
}
