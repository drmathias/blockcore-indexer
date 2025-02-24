using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NBitcoin;

namespace Blockcore.Indexer.Cirrus
{
    public class Bloom : IBitcoinSerializable
    {
        /// <summary>
        /// Length of the bloom data in bytes. 2048 bits.
        /// </summary>
        public const int BloomLength = 256;

        /// <summary>
        /// The actual bloom value represented as a byte array.
        /// </summary>
        private byte[] data;

        public Bloom()
        {
            data = new byte[BloomLength];
        }

        public Bloom(byte[] data)
        {
            if (data?.Length != BloomLength)
                throw new ArgumentException($"Bloom byte array must be {BloomLength} bytes long.", nameof(data));

            this.data = CopyBloom(data);
        }

        /// <summary>
        /// Given this and another bloom, bitwise-OR all the data to get a bloom filter representing a range of data.
        /// </summary>
        public void Or(Bloom bloom)
        {
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] |= bloom.data[i];
            }
        }

        /// <summary>
        /// Add some input to the bloom filter.
        /// </summary>
        /// <remarks>
        ///  From the Ethereum yellow paper (yellowpaper.io):
        ///  M3:2048 is a specialised Bloom filter that sets three bits
        ///  out of 2048, given an arbitrary byte series. It does this through
        ///  taking the low-order 11 bits of each of the first three pairs of
        ///  bytes in a Keccak-256 hash of the byte series.
        /// </remarks>
        public void Add(byte[] input)
        {
           throw new NotImplementedException(); //TODO David check if we need to get the hash helper as well
            // byte[] hashBytes = HashHelper.Keccak256(input);
            // // for first 3 pairs, calculate value of first 11 bits
            // for (int i = 0; i < 6; i += 2)
            // {
            //     uint low8Bits = (uint)hashBytes[i + 1];
            //     uint high3Bits = ((uint)hashBytes[i] << 8) & 2047; // AND with 2047 wipes any bits higher than our desired 11.
            //     uint index = low8Bits + high3Bits;
            //     this.SetBit((int)index);
            //}
        }

        /// <summary>
        /// Determine whether some input is possibly contained within the filter.
        /// </summary>
        /// <param name="test">The byte array to test.</param>
        /// <returns>Whether this data could be contained within the filter.</returns>
        public bool Test(byte[] test)
        {
            var compare = new Bloom();
            compare.Add(test);
            return Test(compare);
        }

        /// <summary>
        /// Determine whether a second bloom is possibly contained within the filter.
        /// </summary>
        /// <param name="bloom">The second bloom to test.</param>
        /// <returns>Whether this data could be contained within the filter.</returns>
        public bool Test(Bloom bloom)
        {
            var copy = new Bloom(bloom.ToBytes());
            copy.Or(this);
            return Equals(copy);
        }

        /// <summary>
        /// Sets the specific bit to 1 within our 256-byte array.
        /// </summary>
        /// <param name="index">Index (0-2047) of the bit to assign to 1.</param>
        private void SetBit(int index)
        {
            int byteIndex = index / 8;
            int bitInByteIndex = index % 8;
            byte mask = (byte)(1 << bitInByteIndex);
            data[byteIndex] |= mask;
        }

        public void ReadWrite(BitcoinStream stream)
        {
            if (stream.Serializing)
            {
                byte[] b = CopyBloom(data);
                stream.ReadWrite(ref b);
            }
            else
            {
                byte[] b = new byte[BloomLength];
                stream.ReadWrite(ref b);
                data = b;
            }
        }

        /// <summary>
        /// Returns the raw bytes of this filter.
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            return CopyBloom(data);
        }

        public override string ToString()
        {
           return Convert.ToString(data); //TODO David validate this
        }

        public static bool operator ==(Bloom obj1, Bloom obj2)
        {
            if (object.ReferenceEquals(obj1, null))
                return object.ReferenceEquals(obj2, null);

            return Enumerable.SequenceEqual(obj1.data, obj2.data);
        }

        public static bool operator !=(Bloom obj1, Bloom obj2)
        {
            return !(obj1 == obj2);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Bloom);
        }

        public bool Equals(Bloom obj)
        {
            if (object.ReferenceEquals(obj, null))
                return false;

            if (object.ReferenceEquals(this, obj))
                return true;

            return (obj == this);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(data);
        }

        private static byte[] CopyBloom(byte[] bloom)
        {
            byte[] result = new byte[BloomLength];
            Buffer.BlockCopy(bloom, 0, result, 0, BloomLength);
            return result;
        }
    }
}
