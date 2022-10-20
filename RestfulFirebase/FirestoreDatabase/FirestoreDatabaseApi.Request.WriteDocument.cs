using System;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.Common.Utilities;
using RestfulFirebase.FirestoreDatabase.Utilities;
using System.Linq;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.Common.Http;
using System.Threading;
using RestfulFirebase.Common.Abstractions;
using System.Collections;
using System.Collections.Generic;
using RestfulFirebase.FirestoreDatabase.Writes;

namespace RestfulFirebase.FirestoreDatabase;

public partial class FirestoreDatabaseApi
{
    /// <summary>
    /// Creates a new write commit.
    /// </summary>
    /// <returns>
    /// The newly created <see cref="WriteRoot"/>.
    /// </returns>
    public WriteRoot Write()
    {
        return new WriteRoot(App);
    }
}
