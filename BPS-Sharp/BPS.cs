using System;
using System.IO;
using System.Threading.Tasks;
/*  Original by Alcaro

    https://media.smwcentral.net/Alcaro/bps/
    https://www.smwcentral.net/?p=profile&id=1686

    C# port by GlitchyPSI */
namespace BPS_Sharp
{
    static class BPS
    {

        static public async Task<byte[]> TryBPS(byte[] ROM, byte[] Patch, string path)
        {
            byte[] ret;
            ret = await ApplyBPS(ROM, Patch);
            File.WriteAllBytes(path, ret);
            return ret;
        }

        static async Task<byte[]> ApplyBPS(byte[] ROM, byte[] Patch)
        {
            return await Task.Run(() =>
            {
                Crc32 crc32 = new Crc32();
                long patchpos = 0;
                int u8() { return Patch[patchpos++]; }
                int u32at(int pos) { return (Patch[pos + 0] << 0 | Patch[pos + 1] << 8 | Patch[pos + 2] << 16 | Patch[pos + 3] << 24) >> 0; }

                long decode()
                {
                    int ret = 0;
                    int sh = 0;
                    while (true)
                    {
                        int next = u8();
                        ret += (next ^ 0x80) << sh;
                        if ((next & 0x80) == 128) return ret;
                        sh += 7;
                    }
                }

                long decodes()
                {
                    long enc = decode();
                    long ret = enc >> 1;
                    if ((enc & 1) != 0)
                    {
                        ret = -ret;
                    }
                    return ret;
                }

                if (u8() != 0x42 || u8() != 0x50 || u8() != 0x53 || u8() != 0x31)
                {
                    throw new WrongPatchException();
                }
                if (decode() != ROM.Length)
                {
                    throw new WrongFileException(decode(), ROM.Length);
                }
                if (crc32.ComputeChecksum(ROM) != u32at(Patch.Length - 12))
                {
                    throw new WrongFileException();
                }

                byte[] _out = new byte[decode()];
                int outpos = 0;

                switch (outpos)
                {
                    case (0):
                        break;
                }
                long inreadpos = 0;
                long outreadpos = 0;
                while (patchpos < Patch.Length - 12)
                {
                    long thisinstr = decode();
                    long len = (thisinstr >> 2) + 1;
                    int action = (int)(thisinstr & 3);
                    switch (action)
                    {
                        case (0):
                            {
                                for (int i = 0; i < len; i++)
                                {
                                    _out[outpos] = ROM[outpos];
                                    outpos++;
                                }
                            }
                            break;
                        case (1):
                            {
                                for (int i = 0; i < len; i++)
                                {
                                    _out[outpos++] = (byte)u8();
                                }
                            }
                            break;
                        case (2):
                            {

                                inreadpos += decodes();
                                for (int i = 0; i < len; i++)
                                {
                                    _out[outpos++] = ROM[inreadpos++];
                                }
                            }
                            break;
                        case (3):
                            {

                                outreadpos += decodes();
                                for (int i = 0; i < len; i++)
                                {
                                    _out[outpos++] = _out[outreadpos++];
                                }
                            }
                            break;
                    };
                }
                return _out;
            });

        }

        class WrongFileException : Exception
        {
            static long decsize;
            static long romsize;
            public WrongFileException(long decoded, long rsize) : base("Wrong file. Checksums do not match.")
            {
                decsize = decoded;
                romsize = rsize;

            }
            public WrongFileException() : base("Wrong file. Expected ROMsize of " + romsize + ", got " + decsize)
            {
            }
        }

        class WrongPatchException : Exception
        {
            public WrongPatchException() : base("The specified patch file isn't a BPS patch.")
            {

            }
        }

    }

    public class Crc32
    {
        uint[] table;

        public uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0xffffffff;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(((crc) & 0xff) ^ bytes[i]);
                crc = ((crc >> 8) ^ table[index]);
            }
            return ~crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            return BitConverter.GetBytes(ComputeChecksum(bytes));
        }

        public Crc32()
        {
            uint poly = 0xedb88320;
            table = new uint[256];
            uint temp = 0;
            for (uint i = 0; i < table.Length; ++i)
            {
                temp = i;
                for (int j = 8; j > 0; --j)
                {
                    if ((temp & 1) == 1)
                    {
                        temp = ((temp >> 1) ^ poly);
                    }
                    else
                    {
                        temp >>= 1;
                    }
                }
                table[i] = temp;
            }
        }
    }
}