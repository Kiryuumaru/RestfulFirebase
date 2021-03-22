using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class DistinctObject : ObservableObject
    {
        #region Properties

        public string Key
        {
            get => GetAttribute<string>(nameof(Key), nameof(DistinctProperty)).Value;
            private set => SetAttribute(nameof(Key), nameof(DistinctProperty), value);
        }

        #endregion

        #region Initializers

        public static DistinctObject Create()
        {
            var obj = new DistinctObject(null)
            {
                Key = Helpers.GenerateSafeUID()
            };
            return obj;
        }

        public static DistinctObject CreateFromKey(string key)
        {
            var obj = new DistinctObject(null)
            {
                Key = key
            };
            return obj;
        }

        public static DistinctObject CreateFromKeyAndProperties(string key, IEnumerable<DistinctProperty> properties)
        {
            var obj = new DistinctObject(CreateFromProperties(properties))
            {
                Key = key
            };
            return obj;
        }

        public DistinctObject(AttributeHolder holder) : base(holder)
        {

        }

        #endregion

        #region Methods



        #endregion
    }
}
