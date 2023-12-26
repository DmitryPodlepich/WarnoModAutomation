using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WarnoModeAutomation.DTO
{
    public class NDFFileTypeInfo<T> where T : Descriptor
    {
        public readonly T FileType;
        public readonly string FileName;

        public NDFFileTypeInfo(string fileName, T fileType)
        {
            FileName = fileName;
            FileType = fileType;
        }
    }
}
