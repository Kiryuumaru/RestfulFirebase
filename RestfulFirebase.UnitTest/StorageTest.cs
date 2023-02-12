using Xunit;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.Common.Models;
using System.Collections.Generic;
using System.Linq;
using RestfulFirebase.Common.Utilities;
using System.Threading.Tasks;
using System.IO;

namespace RestfulFirebase.UnitTest;

public class StorageTest
{
    [Fact]
    public async void UploadGetDeleteTest()
    {
        FirebaseApp app = Helpers.GetFirebaseApp();

        var fileLocation = app.Storage.Bucket()
            .Child(nameof(StorageTest))
            .Child(nameof(UploadGetDeleteTest))
            .Child("image.jpg");

        FileStream img = File.OpenRead("image.jpg");

        int percent = 0;
        var writeTask = fileLocation.Write(img);
        writeTask.Progress.ProgressChanged += (s, e) =>
        {
            percent = e.Percentage;
        };

        (await writeTask).ThrowIfError();

        (await fileLocation.GetDownloadUrl()).ThrowIfError();

        (await fileLocation.Delete()).ThrowIfError();

        await Task.Delay(1);
    }
}
