using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using ImgHash;

namespace ImgDup.Net
{
    class ImageHasher
    {
        private BlockingCollection<HashedImage> hashedImages;
        private ParallelLoopResult status;

        public readonly int Total;

        public bool IsCompleted
        {
            get { return this.status.IsCompleted;  }
        }

        public int Progress
        {
            get { return this.hashedImages.Count; }
        }

        private ImageHasher(BlockingCollection<HashedImage> hashedImages, ParallelLoopResult status, int total)
        {
            this.hashedImages = hashedImages;
            this.status = status;
            this.Total = total;
        }

        public static ImageHasher Start(string[] imagesToHash, ImgHash.ImgHash.ExternHashType hashType, uint hashSize)
        {
            var hashedImages = new BlockingCollection<HashedImage>();
            var total = imagesToHash.Length;
            var status = Parallel.ForEach(imagesToHash, (image) => HashImage(image, hashedImages, hashType, hashSize));
            return new ImageHasher(hashedImages, status, total);
        }

        private static void HashImage(string image, BlockingCollection<HashedImage> output, ImgHash.ImgHash.ExternHashType hashType, uint hashSize)
        {
            var bmp = new Bitmap(image);
            var hash = ImgHash.ImgHash.Create(bmp, hashType, hashSize);
            output.Add(new HashedImage(image, hash));
            output.CompleteAdding();
        }
    }

    class HashedImage
    {
        public HashedImage(string path, ImgHash.ImgHash hash)
        {
            this.Path = path;
            this.Hash = hash;
        }

        public readonly string Path;
        public readonly ImgHash.ImgHash Hash;
    }
}
