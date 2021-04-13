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

    public class ObservableObjectInternalChangesEventArgs : PropertyChangedEventArgs
    {
        public PropertyChangeType Type { get; }
        public string Key { get; }
        public string Group { get; }
        public ObservableObjectInternalChangesEventArgs(
            PropertyChangeType type,
            string key,
            string group,
            string propertyName)
            : base(propertyName)
        {
            Type = type;
            Key = key;
            Group = group;
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
