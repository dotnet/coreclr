// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
** 
**
**
** Purpose: A collection of methods for manipulating Files.
**
**        April 09,2000 (some design refactorization)
**
===========================================================*/

using Win32Native = Microsoft.Win32.Win32Native;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace System.IO
{
    // Class for creating FileStream objects, and some basic file management
    // routines such as Delete, etc.
    internal static class File
    {
        private const int ERROR_INVALID_PARAMETER = 87;
        internal const int GENERIC_READ = unchecked((int)0x80000000);

        private const int GetFileExInfoStandard = 0;

        // Tests if a file exists. The result is true if the file
        // given by the specified path exists; otherwise, the result is
        // false.  Note that if path describes a directory,
        // Exists will return true.
        public static bool Exists(String path)
        {
            return InternalExistsHelper(path);
        }

        private static bool InternalExistsHelper(String path)
        {
            try
            {
                if (path == null)
                    return false;
                if (path.Length == 0)
                    return false;

                path = Path.GetFullPath(path);

                // After normalizing, check whether path ends in directory separator.
                // Otherwise, FillAttributeInfo removes it and we may return a false positive.
                // GetFullPath should never return null
                Debug.Assert(path != null, "File.Exists: GetFullPath returned null");
                if (path.Length > 0 && PathInternal.IsDirectorySeparator(path[path.Length - 1]))
                {
                    return false;
                }

                return InternalExists(path);
            }
            catch (ArgumentException) { }
            catch (NotSupportedException) { } // Security can throw this on ":"
            catch (SecurityException) { }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            return false;
        }

        internal static bool InternalExists(String path)
        {
            Win32Native.WIN32_FILE_ATTRIBUTE_DATA data = new Win32Native.WIN32_FILE_ATTRIBUTE_DATA();
            int dataInitialised = FillAttributeInfo(path, ref data, false, true);

            return (dataInitialised == 0) && (data.fileAttributes != -1)
                    && ((data.fileAttributes & Win32Native.FILE_ATTRIBUTE_DIRECTORY) == 0);
        }

        public static byte[] ReadAllBytes(String path)
        {
            byte[] bytes;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                FileStream.DefaultBufferSize, FileOptions.None))
            {
                // Do a blocking read
                int index = 0;
                long fileLength = fs.Length;
                if (fileLength > Int32.MaxValue)
                    throw new IOException(SR.IO_FileTooLong2GB);
                int count = (int)fileLength;
                bytes = new byte[count];
                while (count > 0)
                {
                    int n = fs.Read(bytes, index, count);
                    if (n == 0)
                        __Error.EndOfFile();
                    index += n;
                    count -= n;
                }
            }
            return bytes;
        }

#if PLATFORM_UNIX
        public static String[] ReadAllLines(String path)
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (path.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyPath);
            Contract.EndContractBlock();

            return InternalReadAllLines(path, Encoding.UTF8);
        }

        private static String[] InternalReadAllLines(String path, Encoding encoding)
        {
            Contract.Requires(path != null);
            Contract.Requires(encoding != null);
            Contract.Requires(path.Length != 0);

            String line;
            List<String> lines = new List<String>();

            using (StreamReader sr = new StreamReader(path, encoding))
                while ((line = sr.ReadLine()) != null)
                    lines.Add(line);

            return lines.ToArray();
        }
#endif // PLATFORM_UNIX

        // Returns 0 on success, otherwise a Win32 error code.  Note that
        // classes should use -1 as the uninitialized state for dataInitialized.
        internal static int FillAttributeInfo(String path, ref Win32Native.WIN32_FILE_ATTRIBUTE_DATA data, bool tryagain, bool returnErrorOnNotFound)
        {
            int dataInitialised = 0;
            if (tryagain) // someone has a handle to the file open, or other error
            {
                Win32Native.WIN32_FIND_DATA findData;
                findData = new Win32Native.WIN32_FIND_DATA();

                // Remove trialing slash since this can cause grief to FindFirstFile. You will get an invalid argument error
                String tempPath = path.TrimEnd(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });

                // For floppy drives, normally the OS will pop up a dialog saying
                // there is no disk in drive A:, please insert one.  We don't want that.
                // SetErrorMode will let us disable this, but we should set the error
                // mode back, since this may have wide-ranging effects.
                int oldMode = Win32Native.SetErrorMode(Win32Native.SEM_FAILCRITICALERRORS);
                try
                {
                    bool error = false;
                    SafeFindHandle handle = Win32Native.FindFirstFile(tempPath, findData);
                    try
                    {
                        if (handle.IsInvalid)
                        {
                            error = true;
                            dataInitialised = Marshal.GetLastWin32Error();

                            if (dataInitialised == Win32Native.ERROR_FILE_NOT_FOUND ||
                                dataInitialised == Win32Native.ERROR_PATH_NOT_FOUND ||
                                dataInitialised == Win32Native.ERROR_NOT_READY)  // floppy device not ready
                            {
                                if (!returnErrorOnNotFound)
                                {
                                    // Return default value for backward compatibility
                                    dataInitialised = 0;
                                    data.fileAttributes = -1;
                                }
                            }
                            return dataInitialised;
                        }
                    }
                    finally
                    {
                        // Close the Win32 handle
                        try
                        {
                            handle.Close();
                        }
                        catch
                        {
                            // if we're already returning an error, don't throw another one. 
                            if (!error)
                            {
                                Debug.Assert(false, "File::FillAttributeInfo - FindClose failed!");
                                __Error.WinIOError();
                            }
                        }
                    }
                }
                finally
                {
                    Win32Native.SetErrorMode(oldMode);
                }

                // Copy the information to data
                data.PopulateFrom(findData);
            }
            else
            {
                // For floppy drives, normally the OS will pop up a dialog saying
                // there is no disk in drive A:, please insert one.  We don't want that.
                // SetErrorMode will let us disable this, but we should set the error
                // mode back, since this may have wide-ranging effects.
                bool success = false;
                int oldMode = Win32Native.SetErrorMode(Win32Native.SEM_FAILCRITICALERRORS);
                try
                {
                    success = Win32Native.GetFileAttributesEx(path, GetFileExInfoStandard, ref data);
                }
                finally
                {
                    Win32Native.SetErrorMode(oldMode);
                }

                if (!success)
                {
                    dataInitialised = Marshal.GetLastWin32Error();
                    if (dataInitialised != Win32Native.ERROR_FILE_NOT_FOUND &&
                        dataInitialised != Win32Native.ERROR_PATH_NOT_FOUND &&
                        dataInitialised != Win32Native.ERROR_NOT_READY)  // floppy device not ready
                    {
                        // In case someone latched onto the file. Take the perf hit only for failure
                        return FillAttributeInfo(path, ref data, true, returnErrorOnNotFound);
                    }
                    else
                    {
                        if (!returnErrorOnNotFound)
                        {
                            // Return default value for backward compbatibility
                            dataInitialised = 0;
                            data.fileAttributes = -1;
                        }
                    }
                }
            }

            return dataInitialised;
        }

        // If we use the path-taking constructors we will not have FileOptions.Asynchronous set and
        // we will have asynchronous file access faked by the thread pool. We want the real thing.
        private static StreamReader AsyncStreamReader(string path, Encoding encoding)
        {
            FileStream stream = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read, FileStream.DefaultBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            return new StreamReader(stream, encoding, true);
        }

        private static StreamWriter AsyncStreamWriter(string path, bool append, Encoding encoding)
        {
            FileStream stream = new FileStream(
                path, append ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read,
                FileStream.DefaultBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan);

            return new StreamWriter(stream, encoding);
        }

        public static Task<string> ReadAllTextAsync(
            string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReadAllTextAsync(path, Encoding.UTF8, cancellationToken);
        }

        public static Task<string> ReadAllTextAsync(
            string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (path.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyPath, nameof(path));

            Contract.EndContractBlock();

            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<string>(cancellationToken)
                : InternalReadAllTextAsync(path, encoding, cancellationToken);
        }

        private static async Task<string> InternalReadAllTextAsync(String path, Encoding encoding, CancellationToken cancel)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));
            Contract.Requires(encoding != null);

            using (StreamReader sr = AsyncStreamReader(path, encoding))
            {
                cancel.ThrowIfCancellationRequested();
                StringBuilder sb = new StringBuilder();
                int bufferSize = sr.CurrentEncoding.GetMaxCharCount(FileStream.DefaultBufferSize);
                char[] buffer = new char[bufferSize];
                for (;;)
                {
                    int read = await sr.ReadAsync(buffer, 0, bufferSize).ConfigureAwait(false);
                    cancel.ThrowIfCancellationRequested();
                    if (read == 0)
                    {
                        return sb.ToString();
                    }

                    sb.Append(buffer, 0, read);
                }
            }
        }

        public static Task WriteAllTextAsync(
            string path, string contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteAllTextAsync(path, contents, StreamWriter.UTF8NoBOM, cancellationToken);
        }

        public static Task WriteAllTextAsync(
            string path, string contents, Encoding encoding,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (path.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyPath, nameof(path));

            Contract.EndContractBlock();

            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : InternalWriteAllTextAsync(AsyncStreamWriter(path, false, encoding), contents, cancellationToken);
        }

        public static async Task<byte[]> ReadAllBytesAsync(
            string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            using (FileStream fs = new FileStream(
                path, FileMode.Open, FileAccess.Read, FileShare.Read, FileStream.DefaultBufferSize,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                long fileLength = fs.Length;
                if (fileLength > Int32.MaxValue)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    throw new IOException(Environment.GetResourceString("IO.IO_FileTooLong2GB"));
                }

                if (fileLength == 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    return Array.Empty<byte>();
                }

                int index = 0;
                int count = (int)fileLength;
                byte[] bytes = new byte[count];
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int n = await fs.ReadAsync(bytes, index, Math.Min(count - index, FileStream.DefaultBufferSize), cancellationToken)
                        .ConfigureAwait(false);

                    if (n == 0)
                    {
                        __Error.EndOfFile();
                    }

                    index += n;
                } while (index < count);

                cancellationToken.ThrowIfCancellationRequested();
                return bytes;
            }
        }

        public static Task WriteAllBytesAsync(
            string path, byte[] bytes, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path), SR.ArgumentNull_Path);
            if (path.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyPath, nameof(path));
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            Contract.EndContractBlock();
            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : InternalWriteAllBytesAsync(path, bytes, cancellationToken);
        }

        private static async Task InternalWriteAllBytesAsync(String path, byte[] bytes, CancellationToken cancel)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));
            Contract.Requires(bytes != null);

            using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read,
                FileStream.DefaultBufferSize, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                cancel.ThrowIfCancellationRequested();
                await fs.WriteAsync(bytes, 0, bytes.Length, cancel).ConfigureAwait(false);
            }
        }

        public static Task<string[]> ReadAllLinesAsync(
            string path, CancellationToken cancellationToken = default(CancellationToken))
        {
            return ReadAllLinesAsync(path, Encoding.UTF8, cancellationToken);
        }

        public static Task<string[]> ReadAllLinesAsync(
            string path, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (path.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyPath, nameof(path));

            Contract.EndContractBlock();

            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled<string[]>(cancellationToken)
                : InternalReadAllLinesAsync(path, encoding, cancellationToken);
        }

        private static async Task<string[]> InternalReadAllLinesAsync(
            String path, Encoding encoding, CancellationToken cancel)
        {
            Contract.Requires(!string.IsNullOrEmpty(path));
            Contract.Requires(encoding != null);

            using (StreamReader sr = AsyncStreamReader(path, encoding))
            {
                cancel.ThrowIfCancellationRequested();
                string line;
                List<string> lines = new List<string>();
                while ((line = await sr.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    cancel.ThrowIfCancellationRequested();
                    lines.Add(line);
                }

                cancel.ThrowIfCancellationRequested();
                return lines.ToArray();
            }
        }

        public static Task WriteAllLinesAsync(
            string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return WriteAllLinesAsync(path, contents, StreamWriter.UTF8NoBOM, cancellationToken);
        }

        public static Task WriteAllLinesAsync(
            string path, IEnumerable<string> contents, Encoding encoding,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (contents == null)
                throw new ArgumentNullException(nameof(contents));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (path.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyPath, nameof(path));

            Contract.EndContractBlock();

            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : InternalWriteAllLinesAsync(AsyncStreamWriter(path, false, encoding), contents, cancellationToken);
        }

        private static async Task InternalWriteAllLinesAsync(TextWriter writer, IEnumerable<String> contents, CancellationToken cancel)
        {
            Contract.Requires(writer != null);
            Contract.Requires(contents != null);

            using (writer)
            {
                foreach (String line in contents)
                {
                    cancel.ThrowIfCancellationRequested();
                    await (line == null ? writer.WriteLineAsync() : writer.WriteLineAsync(line)).ConfigureAwait(false);
                }
            }
        }

        private static async Task InternalWriteAllTextAsync(StreamWriter sw, string contents, CancellationToken cancel)
        {
            using (sw)
            {
                if (!string.IsNullOrEmpty(contents))
                {
                    cancel.ThrowIfCancellationRequested();
                    char[] buffer = new char[FileStream.DefaultBufferSize];
                    int count = contents.Length;
                    int index = 0;
                    while (index < count)
                    {
                        cancel.ThrowIfCancellationRequested();
                        int batchSize = Math.Min(FileStream.DefaultBufferSize, count);
                        contents.CopyTo(index, buffer, 0, batchSize);
                        await sw.WriteAsync(buffer, 0, batchSize).ConfigureAwait(false);
                        index += batchSize;
                    }
                }
            }
        }

        public static Task AppendAllTextAsync(
            string path, string contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return AppendAllTextAsync(path, contents, StreamWriter.UTF8NoBOM, cancellationToken);
        }

        public static Task AppendAllTextAsync(
            string path, string contents, Encoding encoding,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (path.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyPath, nameof(path));

            Contract.EndContractBlock();

            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : InternalWriteAllTextAsync(AsyncStreamWriter(path, true, encoding), contents, cancellationToken);
        }

        public static Task AppendAllLinesAsync(string path, IEnumerable<string> contents, CancellationToken cancellationToken = default(CancellationToken))
        {
            return AppendAllLinesAsync(path, contents, StreamWriter.UTF8NoBOM, cancellationToken);
        }

        public static Task AppendAllLinesAsync(string path, IEnumerable<string> contents, Encoding encoding, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            if (contents == null)
                throw new ArgumentNullException(nameof(contents));
            if (encoding == null)
                throw new ArgumentNullException(nameof(encoding));
            if (path.Length == 0)
                throw new ArgumentException(SR.Argument_EmptyPath, nameof(path));
            Contract.EndContractBlock();

            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : InternalWriteAllLinesAsync(AsyncStreamWriter(path, true, encoding), contents, cancellationToken);
        }
    }
}
