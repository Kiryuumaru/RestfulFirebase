using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public enum PropertyChangeType
    {
        Set, Delete
    }

    public class ObservablePropertyChangesEventArgs : PropertyChangedEventArgs
    {
        public PropertyChangeType Type { get; }
        public bool IsAdditionals { get; }
        public ObservablePropertyChangesEventArgs(PropertyChangeType type, bool isAdditionals, string propertyName = "") : base(propertyName)
        {
            Type = type;
            IsAdditionals = isAdditionals;
        }
    }

    public class ObservableObjectChangesEventArgs : PropertyChangedEventArgs
    {
        public string Key { get; }
        public PropertyChangeType Type { get; }
        public string KeyGroup { get; }
        public ObservableObjectChangesEventArgs(PropertyChangeType type, string key, string group = "", string propertyName = "") : base(propertyName)
        {
            Type = type;
            Key = key;
            KeyGroup = group;
        }
    }

    public class ContinueExceptionEventArgs
    {
        public readonly Exception Exception;
        public bool IgnoreAndContinue { get; set; }

        public ContinueExceptionEventArgs(Exception exception, bool ignoreAndContinue)
        {
            Exception = exception;
            IgnoreAndContinue = ignoreAndContinue;
        }
    }
}
