using System;
using Bond.Protocols;
using Bond.IO.Unsafe;

namespace BondTest
{
    public class BondSerializer : ISerializer<Data>
    {
        public byte[] Serialize(Data obj)
        {
            var output = new OutputBuffer();
            var writer = new CompactBinaryWriter<OutputBuffer>(output);

            Bond.Serialize.To(writer, obj);

            var dest = new byte[output.Data.Count];
            Buffer.BlockCopy(output.Data.Array, 0, dest, 0, output.Data.Count);
            return dest;
        }

        public Data Deserialize(byte[] bytes)
        {
            var input = new InputBuffer(bytes);
            var reader = new CompactBinaryReader<InputBuffer>(input);

            return Bond.Deserialize<Data>.From(reader);
        }
    }
}