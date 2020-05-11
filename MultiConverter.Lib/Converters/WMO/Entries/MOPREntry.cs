using System;
using System.Collections.Generic;
using System.Text;

namespace MultiConverter.Lib.Converters.WMO.Entries
{
    public class MOPREntry
    {
        public ushort PortalIndex { get; set; }
        public ushort WMOGroupIndex { get; set; }
        public short Direction { get; set; }
        public ushort AlwaysZero { get; set; }
    }
}
