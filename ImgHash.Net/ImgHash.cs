using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ImgHash
{
    #region extension methods
    static class Extensions
    {
        public static int GetChannelCount(this PixelFormat pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormat.Format8bppIndexed:
                    return 1;
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format32bppArgb:
                    return -4;
                default:
                    throw new NotSupportedException(
                        String.Format("PixelFormat not supported: {0}", pixelFormat.ToString())
                    );
            }
        }
    }
    #endregion

    public class ImgHash
    {
        #region img-hash-extern symbols
        public enum ExternHashType 
        {
            Mean = 1,
            Gradient = 2,
            DoubleGradient = 3,
            DCT = 4
        }

        struct ExternHashImage { }

        [DllImport("img_hash_extern.dll", EntryPoint="get_hash_data_alloc_size")]
        unsafe static extern UIntPtr get_hash_data_alloc_size(ExternHashType hashType, uint hashSize);
        [DllImport("img_hash_extern.dll", EntryPoint = "create_hash_image")]
        unsafe static extern ExternHashImage* create_hash_image(byte* img_data, uint width, uint height, int channels);
        [DllImport("img_hash_extern.dll", EntryPoint = "create_hash")]
        unsafe static extern int create_hash(ExternHashImage* hashImage, ExternHashType hashType, uint hashSize, byte* hash_data_out);

        #endregion

        private ImgHash(BitArray hashData, ExternHashType hashType, uint hashSize)
        {
            this.hashData = hashData;
            this.hashType = hashType;
            this.hashSize = hashSize;
        }

        #region ImgHash fields

        private BitArray hashData;
        private ExternHashType hashType;
        private uint hashSize;

        #endregion

        #region ImgHash properties
        public BitArray HashData
        {
            get { return this.hashData; }
        }

        public ExternHashType HashType
        {
            get { return this.hashType; }
        }

        public uint HashSize
        {
            get { return this.hashSize;  }
        }
        #endregion

        /// <summary>
        /// Create a hash from the given <see cref="Bitmap" /> using the given hash type and size.
        /// 
        /// Note: if <c>hashType</c> is given as <see cref="ExternHashType.DoubleGradient" />, then the hash will be double the length.
        /// </summary>
        /// <param name="img">The image to hash.</param>
        /// <param name="hashType">The type of hash to perform.</param>
        /// <param name="hashSize">The size of the hash to use. (The hash length will be <c>hashSize * hashSize</c>)</param>
        /// <returns>A new created ImgHash</returns>
        public static ImgHash Create(Bitmap img, ExternHashType hashType, uint hashSize)
        {
            BitArray hashData = new BitArray(CreateHash(img, hashType, hashSize));
            return new ImgHash(hashData, hashType, hashSize);
        }

        /// <summary>
        /// Get the Hamming distance between the bits of this hash and <c>other</c> as an <c>int</c>.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int Dist(ImgHash other) 
        {
            var xor = this.hashData.Xor(other.hashData);
            var count = 0;

            foreach (bool bit in xor)
                if (bit) count++;

            return count;
        }

        private static unsafe byte[] CreateHash(Bitmap img, ExternHashType hashType, uint hashSize)
        {
            int allocSize = (int) get_hash_data_alloc_size(hashType, hashSize);
            var hashData = new byte[allocSize];

            ExternHashImage* hashImage = CreateHashImage(img);

            fixed (byte* hashDataPtr = hashData)
            {
                int result = create_hash(hashImage, hashType, hashSize, hashDataPtr);
            }

            return hashData;
        }

        private static unsafe ExternHashImage* CreateHashImage(Bitmap img)
        {
            var width = img.Width;
            var height = img.Height;
            var rect = new Rectangle(0, 0, width, height);
            var channelCount = img.PixelFormat.GetChannelCount();

            var bmpData = img.LockBits(rect, ImageLockMode.ReadOnly, img.PixelFormat);
            byte* ptr = (byte*)bmpData.Scan0.ToPointer();
            ExternHashImage* hashImage = create_hash_image(ptr, (uint)width, (uint)height, channelCount);
            img.UnlockBits(bmpData);

            return hashImage;
        }
    }
}
