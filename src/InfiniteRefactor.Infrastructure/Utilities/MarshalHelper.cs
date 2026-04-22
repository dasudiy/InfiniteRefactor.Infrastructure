using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace AirMaster.Infrastructure.Utilities
{
    public class MarshalHelper
    {
        public static T FromByteArray<T>(byte[] rawValue) where T : new()
        {
            return (T)FromByteArray(rawValue, typeof(T));
        }

        public static T FromByteArray<T>(byte[] rawValue, int start, int length) where T : new()
        {
            var bytes = new byte[length];
            Array.Copy(rawValue, start, bytes, 0, length);
            return FromByteArray<T>(bytes);
        }

        public static object FromByteArray(byte[] rawValue, Type type)
        {
            RespectEndianness(type, rawValue);
            if (typeof(ICustomBinarySerializer).IsAssignableFrom(type))
            {
                var t = Activator.CreateInstance(type);
                ((ICustomBinarySerializer)t).LoadFromBytes(rawValue);
                return t;
            }
            GCHandle handle = GCHandle.Alloc(rawValue, GCHandleType.Pinned);
            var structure = Marshal.PtrToStructure(handle.AddrOfPinnedObject(), type);
            handle.Free();
            return structure;
        }

        public static byte[] ToByteArray(object value, int maxLength = int.MaxValue)
        {
            if (value is ICaculateLength)
            {
                (value as ICaculateLength).CaculateLength();
            }
            if (value is ICustomBinarySerializer)
            {
                var bytes = ((ICustomBinarySerializer)value).ToBytes();
                RespectEndianness(value.GetType(), bytes);
                return bytes;
            }

            int rawsize = Marshal.SizeOf(value);
            byte[] rawdata = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdata, GCHandleType.Pinned);
            try
            {
                Marshal.StructureToPtr(value, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                handle.Free();
            }

            RespectEndianness(value.GetType(), rawdata);

            if (maxLength < rawdata.Length)
            {
                byte[] temp = new byte[maxLength];
                Array.Copy(rawdata, temp, maxLength);
                return temp;
            }
            else
            {
                return rawdata;
            }
        }

        private static void RespectEndianness(Type type, byte[] data)
        {
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).Where(f => f.IsDefined(typeof(EndianAttribute), false))
                .Select(f =>
                {
                    var attr = (EndianAttribute)f.GetCustomAttributes(typeof(EndianAttribute), false)[0];
                    return new
                    {
                        Field = f,
                        Attribute = attr,
                        Offset = attr.Offset > 0 ? attr.Offset : Marshal.OffsetOf(type, f.Name).ToInt32()
                    };
                }).ToList();

            foreach (var field in fields)
            {
                if ((field.Attribute.Endianness == Endianness.BigEndian && BitConverter.IsLittleEndian) ||
                    (field.Attribute.Endianness == Endianness.LittleEndian && !BitConverter.IsLittleEndian))
                {
                    if (field.Field.IsDefined(typeof(MarshalAsAttribute), false))
                    {
                        var arr = field.Field.GetCustomAttributes(typeof(MarshalAsAttribute), false).FirstOrDefault() as MarshalAsAttribute;
                        if (arr != null)
                        {
                            Array.Reverse(data, field.Offset, arr.SizeConst);
                        }
                    }
                    else
                    {
                        Array.Reverse(data, field.Offset, field.Attribute.Size > 0 ? field.Attribute.Size : Marshal.SizeOf(field.Field.FieldType));
                    }
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class EndianAttribute : Attribute
    {
        public Endianness Endianness { get; private set; }
        public int Size { get; private set; }
        public int Offset { get; internal set; }

        public EndianAttribute(Endianness endianness, int size = 0, int offset = 0)
        {
            this.Endianness = endianness;
            this.Size = size;
            this.Offset = offset;
        }
    }

    public enum Endianness
    {
        BigEndian,
        LittleEndian
    }

    public interface ICustomBinarySerializer
    {
        byte[] ToBytes();
        void LoadFromBytes(byte[] bytes);
    }

    public interface ICaculateLength
    {
        void CaculateLength();
    }
}
