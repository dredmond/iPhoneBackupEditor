using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexViewer
{
    public class BytesNotAvailableException : Exception
    {
        public BytesNotAvailableException(string requestType, int size) 
            : base(string.Format("{0} Failed - {1} bytes are not available.", requestType, size))
        {
            
        }

        public BytesNotAvailableException(string requestType, int size, Exception innerException)
            : base(string.Format("{0} Failed - {1} bytes are not available.", requestType, size), innerException)
        {

        }
    }
}
