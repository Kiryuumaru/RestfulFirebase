using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.CloudFirestore.Models
{
    /// <summary>
    /// Represents a document node of the firebase cloud firestore.
    /// </summary>
    public class Document : IReadOnlyDictionary<string, Field>
    {
        #region Properties

        /// <summary>
        /// Gets the field with the specified fieldName.
        /// </summary>
        /// <param name="fieldName">
        /// The name of the field to get.
        /// </param>
        /// <returns>
        /// The field with the specified <paramref name="fieldName"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="fieldName"/> is a null reference.
        /// </exception>
        public Field this[string fieldName]
        {
            get
            {
                return fields[fieldName];
            }
        }

        /// <summary>
        /// Gets the name of the document.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the time when document is created.
        /// </summary>
        public DateTimeOffset CreateTime { get; }

        /// <summary>
        /// Gets the time when document is updated.
        /// </summary>
        public DateTimeOffset UpdateTime { get; }

        private readonly ConcurrentDictionary<string, Field> fields = new();

        #endregion

        #region Initializers

        /// <summary>
        /// Creates an instance of <see cref="Document"/> that contains the value of the specified firebase cloud firestore document.
        /// </summary>
        /// <param name="name">
        /// The name of the document.
        /// </param>
        /// <param name="createTime">
        /// The time when document is created.
        /// </param>
        /// <param name="updateTime">
        /// The time when document is updated.
        /// </param>
        public Document(string name, DateTimeOffset createTime, DateTimeOffset updateTime)
        {
            Name = name;
            CreateTime = createTime;
            UpdateTime = updateTime;
        }

        #endregion

        #region Methods



        #endregion

        #region IReadOnlyDictionary Members

        /// <inheritdoc/>
        public IEnumerable<string> Keys => fields.Keys;

        /// <inheritdoc/>
        public IEnumerable<Field> Values => fields.Values;

        /// <inheritdoc/>
        public int Count => fields.Count;

        /// <inheritdoc/>
        public bool ContainsKey(string key) => fields.ContainsKey(key);

        /// <inheritdoc/>
        public bool TryGetValue(string key, out Field value) => fields.TryGetValue(key, out value);

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, Field>> GetEnumerator() => fields.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => fields.GetEnumerator();

        #endregion
    }
}
