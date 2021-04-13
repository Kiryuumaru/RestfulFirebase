using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class PropertyFactory
    {
        public Func<(string key, string blob, string tag), DistinctProperty> create;
        public Func<(string key, string blob, string group, string propertyName, string tag), (PropertyHolder propertyHolder, bool hasChanges)> set;
        public Func<(string key, string tag), (PropertyHolder propertyHolder, bool hasChanges)> delete;
        public Func<(string key, string tag), PropertyHolder> get;
        public Func<(string group, string tag), IEnumerable<PropertyHolder>> getAll;

        public PropertyFactory(
            Func<(string key, string blob, string tag), DistinctProperty> create,
            Func<(string key, string blob, string group, string propertyName, string tag), (PropertyHolder propertyHolder, bool hasChanges)> set,
            Func<(string key, string tag), (PropertyHolder propertyHolder, bool hasChanges)> delete,
            Func<(string key, string tag), PropertyHolder> get,
            Func<(string group, string tag), IEnumerable<PropertyHolder>> getAll)
        {
            this.create = create;
            this.set = set;
            this.delete = delete;
            this.get = get;
            this.getAll = getAll;
        }

        public DistinctProperty Create(string key, string blob, string tag = null)
        {
            return create.Invoke((key, blob, tag));
        }

        public (PropertyHolder propertyHolder, bool hasChanges) Set(string key, string blob, string group, string propertyName, string tag = null)
        {
            return set.Invoke((key, blob, group, propertyName, tag));
        }

        public (PropertyHolder propertyHolder, bool hasChanges) Delete(string key, string tag = null)
        {
            return delete.Invoke((key, tag));
        }

        public PropertyHolder Get(string key, string tag = null)
        {
            return get.Invoke((key, tag));
        }

        public IEnumerable<PropertyHolder> GetAll(string group, string tag = null)
        {
            return getAll.Invoke((group, tag));
        }
    }
}
