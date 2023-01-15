TAR Archiver for C# / .NET
==========================

Minimal C# implementation for creating TAR archive (.tar/.tar.gz/.tgz)

This library is built based on the codes from [SharpCompress](https://github.com/adamhathcock/sharpcompress) library with:

- `unsafe` Context Removal
    - `Span<T> = stackalloc T` Remain Untouched &nbsp; <small>* Require C# 7.3 or Later</small>
- Nullables Removal
- `System.Buffers` Dependency Removal
- Unity Ready without additional DLL Installation
- Support for Unity Package Manager



# Sample

<details lang="ja">
<summary><small>日本語</small></summary>

`TarStream` を開いて string、byte[] または Stream を書き込むだけで tar アーカイブが作れます。`System.IO.Compression` と組み合わせて `.tar.gz`（`.tgz`）も作成できます。

</details>


Not complicated. The only feature is creating TAR archive from `string`, `byte[]` or `Stream` without any temporary file creation.

Thanks to `System.IO.Compression` library, you can also create `.tar.gz` (`.tgz`) archive on the fly.



```csharp
using SatorImaging.TarArchiver;
using System;
using System.IO;
using System.IO.Compression;

public class Sample
{
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
```



# Copyright

Copyright &copy; 2023 Sator Imaging, all rights reserved.



# License

<p>
<details>
<summary>The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.</summary>

```text
MIT License

Copyright (c) 2023 Sator Imaging

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

</details>
</p>



## Third-Party Software Notices

**SharpCompress**  
https://github.com/adamhathcock/sharpcompress

<p>
<details>

```text
The MIT License (MIT)

Copyright (c) 2014  Adam Hathcock

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
```

</details>
</p>



# Features

- 🗹 Creating TAR archive from `string`, `byte[]` or `Stream`
- ⊡ Creating TAR archive from local files or directory structure
- ⊡ Appending/Removing files in existing TAR archive
- ⊡ Reading TAR archive contents
- ⊡ Extracting files from TAR archive



# Unity

Tested with Unity 2021.3.

Use the following git URL to import this library using Unity Package Manager (UPM)

- Lastest: `https://github.com/sator-imaging/CSharp-TarArchiver.git`
- v1.0.0:  `https://github.com/sator-imaging/CSharp-TarArchiver.git#v1.0.0`


Note that `src/Sample.cs` will add menu to Unity Editor that shows dialog for exporting test file.

![](https://dl.dropbox.com/s/5qkzw1j4a0ony5a/CSharp-TarArchiver.png)



# Changelog

- Nullables removed.

- Added rethrow code original one doesn't.

- `stackalloc`: removed
- `Span<T> = stackalloc T`: untouched
- `ArrayPool<Byte>`: removed
- `BinaryPrimitives.WriteInt64BigEndian`: polyfilled
