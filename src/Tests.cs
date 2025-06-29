#if DEBUG

using System;
using System.IO;
using System.IO.Compression;

#nullable enable

namespace SatorImaging.TarArchiver
{
    public class Tests
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("TEST/" + nameof(TarArchiver) + "/Create Test File...")]
        static void Create_Test_File()
        {
            var path = UnityEditor.EditorUtility.SaveFilePanel(
                nameof(TarArchiver), string.Empty, "TestFile", @"tar.gz");

            CreateTestFile(new string[] { path });
            UnityEditor.EditorUtility.RevealInFinder(path);
        }
#endif

        static void CreateTestFile(string[] args)
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

            // NOTE: To use MemoryStream instead of FileStream for underlying base stream
            //       of GZipStream, it must be CLOSED before calling MemoryStream.ToArray().
            //       GZipStream.Flush() won't write anything, close it first.
        }
    }
}
#endif
