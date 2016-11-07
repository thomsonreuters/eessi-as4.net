using System.Security.Cryptography;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Eu.EDelivery.AS4.Security.Transforms
{
    internal class AesGcmEncryptTransform : ICryptoTransform
    {
        private readonly GcmBlockCipher _cipher;

        public int InputBlockSize => _cipher.GetBlockSize();
        public int OutputBlockSize => _cipher.GetBlockSize();
        public bool CanTransformMultipleBlocks => false;
        public bool CanReuseTransform => false;

        /// <summary>
        /// Initializes a new instance of the <see cref="AesGcmEncryptTransform"/> class
        /// </summary>
        /// <param name="key"></param>
        /// <param name="iv"></param>
        public AesGcmEncryptTransform(byte[] key, byte[] iv)
        {
            this._cipher = new GcmBlockCipher(new AesFastEngine());

            var parametersWithIv = new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv);
            this._cipher.Init(true, parametersWithIv);
        }

        public void Dispose() {}

        /// <summary>
        /// Transforms the specified region of the input byte array and copies the resulting transform to the specified region of the output byte array.
        /// </summary>
        /// <returns>The number of bytes written.</returns>
        /// <param name="inputBuffer">The input for which to compute the transform. </param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data. </param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data. </param>
        /// <param name="outputBuffer">The output to which to write the transform. </param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data. </param>
        public int TransformBlock(
            byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            return this._cipher.ProcessBytes(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
        }

        /// <summary>
        /// Transforms the specified region of the specified byte array.
        /// </summary>
        /// <returns>The computed transform.</returns>
        /// <param name="inputBuffer">The input for which to compute the transform. </param>
        /// <param name="inputOffset">The offset into the byte array from which to begin using data. </param>
        /// <param name="inputCount">The number of bytes in the byte array to use as data. </param>
        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            var output = new byte[this._cipher.GetOutputSize(inputCount)];

            int numBytesProcessed = this._cipher.ProcessBytes(inputBuffer, inputOffset, inputCount, output, 0);
            this._cipher.DoFinal(output, numBytesProcessed);

            return output;
        }
    }
}