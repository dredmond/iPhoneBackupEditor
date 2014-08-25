using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexViewer
{
    public class FileProperty
    {
        public string Name { get; private set; }
        public byte[] Value { get; private set; }

        public FileProperty(BackupParser parser)
        {
            Name = parser.ReadString();
            Value = parser.ReadStringAsBytes();
        }
    }
}
