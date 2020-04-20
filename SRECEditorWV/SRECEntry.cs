using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRECEditorWV
{
    public class SRECEntry
    {
        public enum SRECEntryType
        {
            Header = 0,
            Data16 = 1,
            Data24 = 2,
            Data32 = 3,
            Reserved = 4,
            Count16 = 5,
            Count24 = 6,
            Start32 = 7,
            Start24 = 8,
            Start16 = 9
        }
        public SRECEntryType type;
        public byte dataSize;
        public uint address;
        public byte[] data;
        public byte crc;

        public SRECEntry()
        {
        }

        public SRECEntry(string line)
        {
            line = line.Trim();
            type = (SRECEntryType)((byte)line[1] - (byte)'0');
            byte[] buff = HexToArray(line.Substring(2));
            byte totalSize = buff[0];
            switch (type)
            {
                case SRECEntryType.Header:
                case SRECEntryType.Data16:
                case SRECEntryType.Count16:
                case SRECEntryType.Start16:
                    address = (uint)((buff[1] << 8) |
                                      buff[2]);
                    break;
                case SRECEntryType.Data24:
                case SRECEntryType.Count24:
                case SRECEntryType.Start24:
                    address = (uint)((buff[1] << 16) |
                                     (buff[2] << 8) |
                                      buff[3]);
                    break;
                case SRECEntryType.Data32:
                case SRECEntryType.Start32:
                    address = (uint)((buff[1] << 24) |
                                     (buff[2] << 16) |
                                     (buff[3] << 8) |
                                      buff[4]);
                    break;
            }
            dataSize = (byte)(totalSize - GetAddressSize() - 1);
            data = new byte[dataSize];
            for (int i = 0; i < dataSize; i++)
                data[i] = buff[1 + GetAddressSize() + i];
            crc = buff[totalSize];
        }

        public void RefreshCRC()
        {
            MemoryStream m = new MemoryStream();
            m.WriteByte((byte)(dataSize + GetAddressSize() + 1));
            switch (GetAddressSize())
            {
                case 2:
                    m.WriteByte((byte)(address >> 8));
                    m.WriteByte((byte)(address));
                    break;
                case 3:
                    m.WriteByte((byte)(address >> 16));
                    m.WriteByte((byte)(address >> 8));
                    m.WriteByte((byte)(address));
                    break;
                case 4:
                    m.WriteByte((byte)(address >> 24));
                    m.WriteByte((byte)(address >> 16));
                    m.WriteByte((byte)(address >> 8));
                    m.WriteByte((byte)(address));
                    break;
            }
            m.Write(data, 0, dataSize);
            crc = MakeCRC(m.ToArray());
        }

        public byte GetAddressSize()
        {
            switch (type)
            {
                case SRECEntryType.Header:
                case SRECEntryType.Data16:
                case SRECEntryType.Count16:
                case SRECEntryType.Start16:
                    return 2;
                case SRECEntryType.Data24:
                case SRECEntryType.Count24:
                case SRECEntryType.Start24:
                    return 3;
                case SRECEntryType.Data32:
                case SRECEntryType.Start32:
                    return 4;
            }
            return 0;
        }

        public string Save()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("S" + (byte)type);
            sb.Append((dataSize + GetAddressSize() + 1).ToString("X2"));
            switch (GetAddressSize())
            {
                case 2:
                    sb.Append(address.ToString("X4"));
                    break;
                case 3:
                    sb.Append(address.ToString("X6"));
                    break;
                case 4:
                    sb.Append(address.ToString("X8"));
                    break;
            }
            foreach (byte b in data)
                sb.Append(b.ToString("X2"));
            sb.Append(crc.ToString("X2"));
            return sb.ToString();
        }

        public string ToListEntry()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(("(" + type.ToString() + ")").PadRight(11));
            sb.Append("S" + (byte)type + " ");
            switch (GetAddressSize())
            {
                case 2:
                    sb.Append("    " + address.ToString("X4") + " ");
                    break;
                case 3:
                    sb.Append("  " + address.ToString("X6") + " ");
                    break;
                case 4:
                    sb.Append(address.ToString("X8") + " ");
                    break;
            }
            string s = "";
            foreach (byte b in data)
                s += b.ToString("X2");
            while (s.Length < 33)
                s += " ";
            sb.Append(s + " " + crc.ToString("X2"));
            return sb.ToString();
        }

        public static byte MakeCRC(byte[] data)
        {
            byte result = 0;
            foreach (byte b in data)
                result += b;
            return (byte)~result;
        }

        public static byte[] HexToArray(string input)
        {
            while (input.Contains(" "))
                input = input.Replace(" ", "");
            MemoryStream m = new MemoryStream();
            for (int i = 0; i < input.Length; i += 2)
                m.WriteByte(Convert.ToByte(input.Substring(i, 2), 16));
            return m.ToArray();
        }
    }
}
