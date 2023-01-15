using SatorImaging.TarArchiver;
using System;
using System.IO;
using System.IO.Compression;


public class Sample
{


#if UNITY_EDITOR
    [UnityEditor.MenuItem(nameof(SatorImaging.TarArchiver) + "/Create Test File")]
    static void CreateTestFile()
    {
        var path = UnityEditor.EditorUtility.SaveFilePanel(nameof(SatorImaging.TarArchiver), "", "TestFile", @"tar.gz");
        Main(new string[] { path });
    }
#endif


    static void Main(string[] args)
    {
        if (args.Length == 0 || string.IsNullOrWhiteSpace(args[0]))
            throw new ArgumentNullException(nameof(args));

        // open TAR stream and export as a .tar.gz format
        using (var targz = new FileStream(args[0], FileMode.Create, FileAccess.Write))
        using (var gzip = new GZipStream(targz, CompressionLevel.Optimal))
        using (var tar = new TarStream(gzip))
        {
            // writing data
            tar.Write(@"path/to/the/file.txt", @"Hello, World.");
            tar.Write(@"path/with/multibyte/文字列.txt", @"ひらがなカタカナ漢字");
            tar.Write(@"yet-another-folder/byteArray.txt", new byte[] { 84, 65, 82, 13, 10 });  //TAR[CR][LF]
            tar.Write(@"root.txt", "");
            tar.Flush();  // done in TarStream.Dispose() anyway
        }

    }
}
