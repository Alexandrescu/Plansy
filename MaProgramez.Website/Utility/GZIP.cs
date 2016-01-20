//using ICSharpCode.SharpZipLib.GZip;

using System;
using System.IO;
using System.IO.Compression;

namespace MaProgramez.Website.Utility
{
    class GZIP
    {
        /// <summary>
        /// The default contructor for the GZIP class.
        /// </summary>
        public GZIP()
        {

        }

        /// <summary>
        /// Compresses a file with GZip format compression.
        /// </summary>
        /// <param name="LocalFile">The local file to compress.</param>
        public void Compress(String LocalFile)
        {
            GZipStream s = new GZipStream(File.Create(LocalFile + ".gz"), CompressionMode.Compress);
            FileStream fStream = File.OpenRead(LocalFile);
            Byte[] writeData = new Byte[fStream.Length];
            fStream.Read(writeData, 0, (int)fStream.Length);
            s.Write(writeData, 0, writeData.Length);
            s.Close();
            fStream.Close();
        }

        /// <summary>
        /// Decompresses a .gz or GZip format file.
        /// </summary>
        /// <param name="LocalFile">The local file to decompress.</param>
        public void Decompress(String LocalFile)
        {
            GZipStream s = new GZipStream(File.OpenRead(LocalFile), CompressionMode.Decompress);

            FileStream fStream = File.Create(Path.GetFileNameWithoutExtension(LocalFile));
            int size = 2048;
            Byte[] writeDate = new Byte[2048];

            while (true)
            {
                size = s.Read(writeDate, 0, size);
                if (size > 0)
                {
                    fStream.Write(writeDate, 0, size);
                }
                else
                {
                    break;
                }
            }
            s.Close();
            fStream.Close();
        }
    }
}
