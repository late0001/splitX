using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace splitX
{
    public class TxtFileSplitPart
    {
        public void FileCut(string sourcePath, string targetFolder, long fileSize)
        {
            if (fileSize <= 0)
            {
                return;
            }
            FileInfo fileInfo = new FileInfo(sourcePath);
            string fileName = fileInfo.Name.Replace(fileInfo.Extension, "");

            FileStream fsRead = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader br = new BinaryReader(fsRead);
            int defaultBurrerLength = 1024 * 1024;
            long fileSizeLength = fileSize * defaultBurrerLength;
            byte[] buffer = new byte[defaultBurrerLength];
            int readLength = 0;
            int fileIndex = 1;
            long fileLength = fileInfo.Length;
            long readFileLength = 0;
            while (readFileLength < fileLength)
            {
                string writeFile = Path.Combine(targetFolder, $"{fileName}_{fileIndex.ToString("D2")}{fileInfo.Extension}");
                FileStream fsWrite = new FileStream(writeFile, FileMode.CreateNew, FileAccess.Write);
                BinaryWriter bw = new BinaryWriter(fsWrite);
                long singleFileLength = 0;
                while ((readLength = br.Read(buffer, 0, buffer.Length)) > 0)
                {
                    bw.Write(buffer, 0, readLength);
                    readFileLength += readLength;
                    singleFileLength += readLength;
                    if (singleFileLength >= fileSizeLength)
                    {
                        bw?.Close();
                        bw?.Dispose();
                        fsWrite?.Close();
                        fsWrite?.Dispose();
                        break;
                    }
                }
                bw?.Close();
                bw?.Dispose();
                fsWrite?.Close();
                fsWrite?.Dispose();
                fileIndex++;
            }
            br?.Close();
            br?.Dispose();
            fsRead?.Close();
            fsRead.Dispose();
        }
    }
}
