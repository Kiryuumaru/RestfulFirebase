using RestfulFirebase.FirestoreDatabase.References;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.FirestoreDatabase.Models
{
    /// <summary>
    /// Represents a document node of the firebase cloud firestore.
    /// </summary>
    public class RealtimeDocument : Document
    {
        #region Properties

        /// <summary>
        /// Gets query of the document.
        /// </summary>
        public DocumentReference Query { get; }

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

        #endregion

        #region Initializers

        internal RealtimeDocument(DocumentReference query, string name, DateTimeOffset createTime, DateTimeOffset updateTime)
        {
            Query = query;
            Name = name;
            CreateTime = createTime;
            UpdateTime = updateTime;
        }

        #endregion

        #region Methods



        #endregion
    }
}
