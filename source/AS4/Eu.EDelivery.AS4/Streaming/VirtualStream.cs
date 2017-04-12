using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Eu.EDelivery.AS4.Streaming
{
    public sealed class VirtualStream : Stream, ICloneable
    {
        private readonly object SyncRoot = new object();

        private const int ThresholdMax = 10485760;
        private const int DefaultSize = 10240;

        private bool _isDisposed;
        private bool _isInMemory;
        private readonly int _threshholdSize;
        private readonly VirtualStream.MemoryFlag _memoryStatus;

        public override bool CanRead
        {
            get
            {
                return UnderlyingStream.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return UnderlyingStream.CanWrite;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return UnderlyingStream.CanSeek;
            }
        }

        public override long Length
        {
            get
            {
                return UnderlyingStream.Length;
            }
        }

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

        public Stream UnderlyingStream { get; private set; }

        public VirtualStream()
          : this(DefaultSize, MemoryFlag.AutoOverFlowToDisk, new MemoryStream())
        {
        }

        public VirtualStream(int bufferSize)
          : this(bufferSize, MemoryFlag.AutoOverFlowToDisk, new MemoryStream(bufferSize))
        {
        }

        public VirtualStream(int bufferSize, int thresholdSize)
          : this(bufferSize, thresholdSize, MemoryFlag.AutoOverFlowToDisk, new MemoryStream(bufferSize))
        {
        }

        public VirtualStream(MemoryFlag flag)
          : this(DefaultSize, flag, flag == MemoryFlag.OnlyToDisk ? CreatePersistentStream(DefaultSize) : new MemoryStream())
        {
        }

        public VirtualStream(int bufferSize, MemoryFlag flag)
          : this(bufferSize, flag, flag == MemoryFlag.OnlyToDisk ? CreatePersistentStream(bufferSize) : new MemoryStream(bufferSize))
        {
        }

        public VirtualStream(Stream dataStream)
          : this(DefaultSize, MemoryFlag.AutoOverFlowToDisk, dataStream)
        {
        }

        private VirtualStream(int bufferSize, MemoryFlag flag, Stream dataStream)
          : this(bufferSize, bufferSize, flag, dataStream)
        {
        }

        private VirtualStream(int bufferSize, int thresholdSize, MemoryFlag flag, Stream dataStream)
        {
            if (dataStream == null)
            {
                throw new ArgumentNullException(nameof(dataStream));
            }

            _isInMemory = flag != MemoryFlag.OnlyToDisk;
            _memoryStatus = flag;
            bufferSize = Math.Min(bufferSize, ThresholdMax);
            _threshholdSize = thresholdSize;
            UnderlyingStream = !_isInMemory ? new BufferedStream(dataStream, bufferSize) : dataStream;
            _isDisposed = false;
        }

        public object Clone()
        {
            if (_isInMemory)
            {
                Stream stream = new MemoryStream((int)UnderlyingStream.Length);
                CopyStreamContentHelper(UnderlyingStream, stream);
                stream.Position = 0L;
                return new VirtualStream(_threshholdSize, MemoryFlag.AutoOverFlowToDisk, stream);
            }

            Stream persistentStream = CreatePersistentStream(_threshholdSize);
            CopyStreamContentHelper(UnderlyingStream, persistentStream);
            persistentStream.Position = 0L;

            return new VirtualStream(_threshholdSize, MemoryFlag.OnlyToDisk, persistentStream);
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
                Stream persistentStream = CreatePersistentStream(_threshholdSize);
                CopyStreamContent((MemoryStream)UnderlyingStream, persistentStream);
                UnderlyingStream = persistentStream;
                _isInMemory = false;
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
                Stream persistentStream = CreatePersistentStream(_threshholdSize);
                CopyStreamContent((MemoryStream)UnderlyingStream, persistentStream);
                UnderlyingStream = persistentStream;
                _isInMemory = false;
                UnderlyingStream.Write(buffer, offset, count);
            }
            else
            {
                UnderlyingStream.Write(buffer, offset, count);
            }
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
            UnderlyingStream = null;
        }

        private void CopyStreamContent(MemoryStream source, Stream target)
        {
            if (source.Length < int.MaxValue)
            {
                long position = source.Position;
                target.Write(source.GetBuffer(), 0, (int)source.Length);
                target.Position = position;
            }
            else
            {
                CopyStreamContentHelper(source, target);
            }
        }

        private void CopyStreamContentHelper(Stream source, Stream target)
        {
            long position = source.Position;
            source.Position = 0L;
            byte[] buffer = new byte[_threshholdSize];
            int count;

            while ((count = source.Read(buffer, 0, _threshholdSize)) != 0)
            {
                target.Write(buffer, 0, count);
            }

            target.Position = position;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed || UnderlyingStream == null)
            {
                throw new ObjectDisposedException("VirtualStream");
            }
        }

        internal static Stream CreatePersistentStream()
        {
            return CreatePersistentStream(DefaultSize);
        }

        internal static Stream CreatePersistentStream(int size)
        {
            StringBuilder stringBuilder = new StringBuilder(261);
            Guid guid = Guid.NewGuid();
            stringBuilder.Append(Path.Combine(Path.GetTempPath(), "VST" + guid.ToString() + ".tmp"));
            return new FileStream(new SafeFileHandle(CreateFile(stringBuilder.ToString(), 3U, 0U, IntPtr.Zero, 2U, 67109120U, IntPtr.Zero), true), FileAccess.ReadWrite, size);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr CreateFile(string name, uint accessMode, uint shareMode, IntPtr security, uint createMode, uint flags, IntPtr template);

        public enum MemoryFlag
        {
            AutoOverFlowToDisk,
            OnlyInMemory,
            OnlyToDisk,
        }
    }
}
