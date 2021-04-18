using RestfulFirebase.Common.Converters;
using RestfulFirebase.Common.Converters.Additionals;
using RestfulFirebase.Common.Converters.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Observables
{
    public class ValueHolder
    {
        #region Properties

        private ValueFactory valueFactory;
        public ValueFactory ValueFactory
        {
            get
            {
                if (valueFactory == null)
                {
                    object valueHolder = null;
                    valueFactory = new ValueFactory(
                        args =>
                        {
                            if (valueHolder != args.value)
                            {
                                valueHolder = args.value;
                                return true;
                            }
                            return false;
                        },
                        args =>
                        {
                            return valueHolder == null ? args.defaultValue : valueHolder;
                        });
                }
                return valueFactory;
            }
            set => valueFactory = value;
        }

        public object Value { get; private set; }

        #endregion

        #region Initializers

        public ValueHolder()
        {

        }

        #endregion

        #region Methods

        public virtual bool SetNull(string tag = null)
        {
            return ValueFactory.Set<object>(null, tag);
        }

        public virtual bool SetValue<T>(T value, string tag = null)
        {
            return ValueFactory.Set(value, tag);
        }

        public virtual T GetValue<T>(T defaultValue = default, string tag = null)
        {
            return ValueFactory.Get(defaultValue, tag);
        }

        #endregion
    }
}
