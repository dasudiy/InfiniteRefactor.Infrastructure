using System;
using System.Runtime.InteropServices;
using InfiniteRefactor.Infrastructure.Utilities;

namespace InfiniteRefactor.Infrastructure.DataService.Bindings.Stream.Infrastructure
{
    public class Packet : ICaculateLength, ICustomBinarySerializer
    {
        public PacketHeader Header;
        public byte[] Content;

        public void CaculateLength()
        {
            Header.Length = (uint)Marshal.SizeOf(Header) + (uint)Content.Length;
        }

        public byte[] ToBytes()
        {
            CaculateLength();
            var buffer = new byte[Header.Length];
            var header = MarshalHelper.ToByteArray(Header);
            Array.Copy(header, buffer, header.Length);
            Array.Copy(Content, 0, buffer, header.Length, Content.Length);

            return buffer;
        }

        public void LoadFromBytes(byte[] bytes)
        {
            var headerLength = Marshal.SizeOf(typeof(PacketHeader));
            Header = MarshalHelper.FromByteArray<PacketHeader>(bytes, 0, headerLength);
            Content = new byte[bytes.Length - headerLength];
            Array.Copy(bytes, headerLength, Content, 0, bytes.Length - headerLength);
        }
    }
}
