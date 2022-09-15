using ObservableHelpers.ComponentModel;
using ObservableHelpers.ComponentModel.Enums;
using RestfulFirebase.FirestoreDatabase.Query;
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
    [ObservableObject]
    public partial class Document<T>
         where T : class
    {
        #region Properties

        /// <summary>
        /// Gets the name of the document node.
        /// </summary>
        [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
        string name;

        /// <summary>
        /// Gets the reference of the document node.
        /// </summary>
        [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
        DocumentReference reference;

        /// <summary>
        /// Gets the <typeparamref name="T"/> model of the document.
        /// </summary>
        [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
        T model;

        /// <summary>
        /// Gets the <see cref="DateTimeOffset"/> create time of the document node.
        /// </summary>
        [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
        DateTimeOffset createTime;

        /// <summary>
        /// Gets the <see cref="DateTimeOffset"/> update time of the document node.
        /// </summary>
        [ObservableProperty(Access = AccessModifier.PublicWithInternalSetter)]
        DateTimeOffset updateTime;

        #endregion

        #region Initializers

        internal Document(string name, DocumentReference reference, T model, DateTimeOffset createTime, DateTimeOffset updateTime)
        {
            this.name = name;
            this.reference = reference;
            this.model = model;
            this.createTime = createTime;
            this.updateTime = updateTime;
        }

        #endregion

        #region Methods



        #endregion
    }
}
