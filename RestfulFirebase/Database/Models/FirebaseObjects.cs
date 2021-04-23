using RestfulFirebase.Common.Models;
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

        public string Key
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }

        public SmallDateTime Modified => throw new NotImplementedException();

        #endregion

        #region Initializers

        public FirebaseObjects(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebaseObjects(string key)
            : base()
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

        public void MakeRealtime(RealtimeWire wire)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public T ParseModel<T>()
            where T : FirebaseObjects
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
