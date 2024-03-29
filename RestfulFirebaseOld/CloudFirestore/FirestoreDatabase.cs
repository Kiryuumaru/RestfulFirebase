﻿using System;
using RestfulFirebase.FirestoreDatabase.References;
using DisposableHelpers;

namespace RestfulFirebase.FirestoreDatabase;

/// <summary>
/// The database instance of the firestore.
/// </summary>
public class FirestoreDatabase : Disposable
{
    #region Properties

    /// <summary>
    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
    /// </summary>
    public RestfulFirebaseApp App { get; }

    /// <summary>
    /// Gets the database id of the firestore.
    /// </summary>
    public string DatabaseId { get; }

    #endregion

    #region Initializers

    internal FirestoreDatabase(RestfulFirebaseApp app, string databaseId)
    {
        App = app;
        DatabaseId = databaseId;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Creates a root collection reference <see cref="CollectionReference"/>.
    /// </summary>
    /// <param name="collectionId">
    /// The ID of the collection reference.
    /// </param>
    /// <returns>
    /// The <see cref="CollectionReference"/> of the specified <paramref name="collectionId"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="collectionId"/> is a <c>null</c> reference.
    /// </exception>
    public CollectionReference Collection(string collectionId)
    {
        if (collectionId == null)
        {
            throw new ArgumentNullException(nameof(collectionId));
        }

        return new CollectionReference(App, this, null, collectionId);
    }

    #endregion

    #region Disposable Members



    #endregion
}
