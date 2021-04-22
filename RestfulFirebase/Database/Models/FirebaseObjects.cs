﻿using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjects : ObservableObjects, IRealtimeModel
    {
        #region Properties

        public string Key { get; protected set; }

        public SmallDateTime Modified => throw new NotImplementedException();

        public RealtimeWire RealtimeWire => throw new NotImplementedException();

        public FirebaseQuery Query => throw new NotImplementedException();

        public bool HasFirstStream => throw new NotImplementedException();

        #endregion

        #region Initializers

        public FirebaseObjects(string key) : base()
        {
            Key = key;
        }

        #endregion

        #region Methods

        public void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableObject property), bool> customValueSetter = null)
        {
            base.SetProperty(value, key, nameof(FirebaseObjects), propertyName, validateValue, customValueSetter);
        }

        public T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T value, ObservableObject property), bool> customValueSetter = null)
        {
            return base.GetProperty(key, nameof(FirebaseObjects), defaultValue, propertyName, customValueSetter);
        }

        public void StartRealtime(FirebaseQuery query)
        {
            throw new NotImplementedException();
        }

        public void StopRealtime()
        {
            throw new NotImplementedException();
        }

        public bool ConsumeStream(StreamObject streamObject)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
