using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class DistinctGroup<T> : ObservableGroup<T>
    {
        #region Properties

        public string Key
        {
            get => Holder.GetAttribute<string>(nameof(Key), nameof(DistinctProperty)).Value;
            private set => Holder.SetAttribute(nameof(Key), nameof(DistinctProperty), value);
        }

        #endregion

        #region Initializers

        public static DistinctGroup<T> Create()
        {
            var obj = new DistinctGroup<T>(null)
            {
                Key = Helpers.GenerateSafeUID()
            };
            return obj;
        }

        public static DistinctGroup<T> CreateFromKey(string key)
        {
            var obj = new DistinctGroup<T>(null)
            {
                Key = key
            };
            return obj;
        }

        public static DistinctGroup<T> CreateFromKeyAndEnumerable(string key, IEnumerable<T> properties)
        {
            var obj = new DistinctGroup<T>(null)
            {
                Key = key
            };
            obj.AddRange(properties, System.Collections.Specialized.NotifyCollectionChangedAction.Add);
            return obj;
        }

        public DistinctGroup(IAttributed attributed)
            : base(attributed)
        {

        }

        #endregion

        #region Methods



        #endregion
    }
}
