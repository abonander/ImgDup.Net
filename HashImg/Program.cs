using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

using ImgHash;

namespace HashImg
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = args[0];
            var bmp = new Bitmap(file);
            var hash = ImgHash.ImgHash.Create(bmp, ImgHash.ImgHash.ExternHashType.Gradient, 16);
                                    
            byte[] hashData = new byte[hash.HashData.Length / 8 + 1];
            hash.HashData.CopyTo(hashData, 0);

            var hashString = System.Convert.ToBase64String(hashData);

            Console.WriteLine("Image hash: {0}", hashString);
        }
    }
}
