using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Eu.EDelivery.AS4.Streaming
{
    public sealed class VirtualStream : Stream, ICloneable
    {
        /// <summary>
        /// The threshold at which VirtualStream will start overflowing to a persistent stream.
        /// </summary>
        public const int ThresholdMax = ThresholdSize.FiftyMegabytes;

        public const int DefaultBufferSize = 8192; // 131072; // -> 128kb // 8192;

        private bool _isDisposed;
        private bool _isInMemory;

        /// <summary>
        /// Determines the Treshold at which size the stream will overflow to disk.
        /// </summary>
        private readonly int _threshholdSize;

        private readonly MemoryFlag _memoryStatus;

        private readonly bool _forAsync;

        /// <summary>
        /// Initializes a new <see cref="VirtualStream"/> instance.
        /// </summary>
        public VirtualStream(bool forAsync = false)
          : this(MemoryFlag.AutoOverFlowToDisk, forAsync)
        { }

        public VirtualStream(MemoryFlag flag, bool forAsync = false)
          : this(ThresholdMax, flag, flag == MemoryFlag.OnlyToDisk ? CreatePersistentStream(forAsync) : new MemoryStream(), forAsync)
        { }

        private VirtualStream(int thresholdSize, MemoryFlag flag, Stream dataStream, bool forAsync)
        {
            if (dataStream == null)
            {
                throw new ArgumentNullException(nameof(dataStream));
            }

            _isInMemory = flag != MemoryFlag.OnlyToDisk;
            _memoryStatus = flag;
            _threshholdSize = thresholdSize;
            _forAsync = forAsync;
            UnderlyingStream = dataStream;
            _isDisposed = false;
        }

        public Stream UnderlyingStream { get; private set; }

        public override bool CanRead => UnderlyingStream.CanRead;

        public override bool CanWrite => UnderlyingStream.CanWrite;

        public override bool CanSeek => UnderlyingStream.CanSeek;

        public override long Length => UnderlyingStream.Length;

        public override long Position
        {
            get
            {
                return UnderlyingStream.Position;
            }
            set
            {
                UnderlyingStream.Seek(value, SeekOrigin.Begin);
            }
        }

        public static VirtualStream CreateVirtualStream(bool forAsync = false)
        {
            return CreateVirtualStream(ThresholdMax, forAsync);
        }

        /// <summary>
        /// Creates a VirtualStream instance.
        /// </summary>
        /// <param name="expectedSize"></param>
        /// <returns></returns>
        public static VirtualStream CreateVirtualStream(long expectedSize, bool forAsync = false)
        {
            if (expectedSize < 0)
            {
                expectedSize = ThresholdMax;
            }

            return CreateVirtualStream(expectedSize, ThresholdMax);
        }

        /// <summary>
        /// Creates a VirtualStream instance.
        /// </summary>
        /// <param name="expectedSize">The expected total size of the stream.</param>
        /// <param name="thresholdSize">The threshold size on which the VirtualStream will be persisted to disk.</param>
        /// <returns></returns>
        public static VirtualStream CreateVirtualStream(long expectedSize, int thresholdSize, bool forAsync = false)
        {
            if (expectedSize > thresholdSize)
            {
                return new VirtualStream(thresholdSize, MemoryFlag.OnlyToDisk, CreatePersistentStream(forAsync), forAsync);
            }

            return new VirtualStream(thresholdSize, MemoryFlag.AutoOverFlowToDisk, new MemoryStream(DefaultBufferSize), forAsync);
        }

        private static Stream CreatePersistentStream(bool forAsync)
        {
            Guid guid = Guid.NewGuid();

            FileOptions options = FileOptions.DeleteOnClose | FileOptions.SequentialScan;

            if (forAsync)
            {
                options |= FileOptions.Asynchronous;
            }

            var fs = new FileStream(Path.Combine(Path.GetTempPath(), "VST" + guid.ToString() + ".tmp"),
                                    FileMode.Create,
                                    FileAccess.ReadWrite,
                                    FileShare.None,
                                    DefaultBufferSize,
                                    options);

            File.SetAttributes(fs.Name, FileAttributes.Temporary | FileAttributes.NotContentIndexed);

            return fs;
        }

        public object Clone()
        {
            Stream clonedStream;

            if (_isInMemory)
            {
                clonedStream = new MemoryStream((int)UnderlyingStream.Length);
            }
            else
            {
                clonedStream = CreatePersistentStream(_forAsync);
            }

            UnderlyingStream.CopyTo(clonedStream);
            clonedStream.Position = 0L;

            return new VirtualStream(_threshholdSize, _memoryStatus, clonedStream, _forAsync);
        }

        public override void Flush()
        {
            ThrowIfDisposed();
            UnderlyingStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            return UnderlyingStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            ThrowIfDisposed();
            return UnderlyingStream.Seek(offset, origin);
        }

        public override void SetLength(long length)
        {
            ThrowIfDisposed();

            if (_memoryStatus == MemoryFlag.AutoOverFlowToDisk && _isInMemory && length > _threshholdSize)
            {
                OverflowToPersistentStream();
                UnderlyingStream.SetLength(length);
            }
            else
            {
                UnderlyingStream.SetLength(length);
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ThrowIfDisposed();
            if (_memoryStatus == MemoryFlag.AutoOverFlowToDisk && _isInMemory && count + UnderlyingStream.Position > _threshholdSize)
            {
                OverflowToPersistentStream();
                UnderlyingStream.Write(buffer, offset, count);
            }
            else
            {
                UnderlyingStream.Write(buffer, offset, count);
            }
        }

        private void OverflowToPersistentStream()
        {
            Stream persistentStream = CreatePersistentStream(_forAsync);
            UnderlyingStream.Position = 0;
            UnderlyingStream.CopyTo(persistentStream);
            UnderlyingStream = persistentStream;
            _isInMemory = false;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!disposing || _isDisposed)
                {
                    return;
                }
                Cleanup();
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        private void Cleanup()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            if (UnderlyingStream == null)
            {
                return;
            }
            UnderlyingStream.Close();

            var fs = UnderlyingStream as FileStream;
            if (fs != null)
            {
                File.Delete(fs.Name);
            }

            UnderlyingStream = null;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed || UnderlyingStream == null)
            {
                throw new ObjectDisposedException("VirtualStream");
            }
        }

        private static class NativeMethods
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern IntPtr CreateFile(string name, uint accessMode, uint shareMode, IntPtr security, uint createMode, uint flags, IntPtr template);
        }

        public enum MemoryFlag
        {
            AutoOverFlowToDisk,
            OnlyInMemory,
            OnlyToDisk,
        }

        private static class ThresholdSize
        {
            public const int FiftyMegabytes = 52_428_800;
            public const int OneHundredMegabytes = 104_857_600;
            public const int TwoHundredMegabytes = 209_715_200;
            public const int TwoHundredFiftyMegabytes = 262_144_000;
        }

        /// <summary>
        /// To a series of bytes.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            byte[] bytes = GetUnderlyingBytes();
            StreamPositionMover.MovePositionToStreamStart(UnderlyingStream);

            return bytes;
        }

        private byte[] GetUnderlyingBytes()
        {
            if (_isInMemory)
            {
                return (UnderlyingStream as MemoryStream)?.ToArray();
            }

            using (Stream fs = UnderlyingStream)
            {
                var binaryReader = new BinaryReader(fs);
                return binaryReader.ReadBytes((int)fs.Length);
            }
        }
    }

}
