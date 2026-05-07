using System.Runtime.InteropServices;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public class PacketHeader
    {
        public byte MagicNumber = 0xa7;
        public byte ProtocolVersion = 0x01;
        public uint Length;
        public ushort MessageId;
        public byte SequenceIndex;
        public byte TotalPackets;
        public EncodingEnum Encoding;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string AppAgent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] MAC;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Reversed;

        internal bool Valid()
        {
            return MagicNumber == 0xa7;
        }
    }
}
