using RestfulFirebase.CloudFirestore.Query;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace RestfulFirebase.FirestoreDatabase
{
    /// <summary>
    /// Represents a document node of the firebase cloud firestore.
    /// </summary>
    public class Document<T>
         where T : class
    {
        #region Properties

        /// <summary>
        /// Gets the name of the document node.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the reference of the document node.
        /// </summary>
        public DocumentReference Reference { get; internal set; }

        /// <summary>
        /// Gets the <typeparamref name="T"/> model of the document.
        /// </summary>
        public T Model { get; internal set; }

        /// <summary>
        /// Gets the <see cref="DateTimeOffset"/> create time of the document node.
        /// </summary>
        public DateTimeOffset CreateTime { get; internal set; }

        /// <summary>
        /// Gets the <see cref="DateTimeOffset"/> update time of the document node.
        /// </summary>
        public DateTimeOffset UpdateTime { get; internal set; }

        #endregion

        #region Initializers

        internal Document(string name, DocumentReference reference, T model, DateTimeOffset createTime, DateTimeOffset updateTime)
        {
            Name = name;
            Reference = reference;
            Model = model;
            CreateTime = createTime;
            UpdateTime = updateTime;
        }

        #endregion

        #region Methods



        #endregion
    }
}
