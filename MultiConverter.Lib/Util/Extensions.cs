using MultiConverter.Lib.Common;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace MultiConverter.Lib.Util
{
    public static class Extensions
    {
        public static CAaBox ReadCAaBox(this BinaryReader reader)
        {
            return new CAaBox
            {
                Min = reader.ReadC3Vector(),
                Max = reader.ReadC3Vector()
            };
        }
        public static void WriteCAaBox(this BinaryWriter writer, CAaBox box)
        {
            writer.WriteC3Vector(box.Min);
            writer.WriteC3Vector(box.Max);
        }

        public static CArgb ReadCArgb(this BinaryReader reader)
        {
            return new CArgb
            {
                R = reader.ReadByte(),
                G = reader.ReadByte(),
                B = reader.ReadByte(),
                A = reader.ReadByte(),
            };
        }
        public static void WriteCArgb(this BinaryWriter writer, CArgb color)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);
        }

        public static C3Vector ReadC3Vector(this BinaryReader reader)
        {
            return new C3Vector
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),
            };
        }
        public static void WriteC3Vector(this BinaryWriter writer, C3Vector vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Y);
            writer.Write(vector.Z);
        }

        public static C4Plane ReadC4Plane(this BinaryReader reader)
        {
            return new C4Plane
            {
                Normal = reader.ReadC3Vector(),
                Distance = reader.ReadSingle()
            };
        }
        public static void WriteC4Plane(this BinaryWriter writer, C4Plane plane)
        {
            writer.WriteC3Vector(plane.Normal);
            writer.Write(plane.Distance);
        }

        public static T Read<T>(this BinaryReader reader) where T : struct
        {
            byte[] result = reader.ReadBytes(Unsafe.SizeOf<T>());

            return Unsafe.ReadUnaligned<T>(ref result[0]);
        }

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            var sb = new StringBuilder();
            char c;

            while ((c = Convert.ToChar(reader.ReadByte())) != 0)
                sb.Append(c);

            return sb.ToString();
        }
    }
}
