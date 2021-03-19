﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public enum PropertyChangeType
    {
        Set, Delete
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

    public class ObservableExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public ObservableExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}