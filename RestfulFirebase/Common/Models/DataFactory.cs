using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class DataParameter
    {
        private string parameterHolder = "";

        public void SetParameter(string key, string value)
        {
            parameterHolder = Helpers.BlobSetValue(parameterHolder, key, value);
        }

        public string GetParameter(string key, string defaultValue = "")
        {
            return Helpers.BlobGetValue(parameterHolder, key, defaultValue);
        }

        public void DeleteParameter(string key)
        {
            parameterHolder = Helpers.BlobDeleteValue(parameterHolder, key);
        }
    }

    public class DataFactory
    {
        public Action<(DataParameter Parameter, string Value)> Set { get; private set; }
        public Func<DataParameter, string> Get { get; private set; }
        public DataFactory(Action<(DataParameter Parameter, string Value)> set, Func<DataParameter, string> get)
        {
            Set = set;
            Get = get;
        }
    }
}
