using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using PVRTexLibNET;

namespace PVRTexConverter.Formats
{
    public class PVR
    {
        public bool isTexture = false;
        public long DataPosition;

        public Header header;
        private byte[] mainTexData;
        private UInt32 bpp;
        public PVR(byte[] input)
        {
            using (MemoryStream ms = new MemoryStream(input))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    header = new Header();
                    UInt32 m_ver = br.ReadUInt32();
                    if (m_ver != 0x03525650)
                    {
                        isTexture = false;
                        return;
                    }
                    isTexture = true;
                    header.Version = m_ver;
                    header.Flags = br.ReadUInt32();
                    UInt64 mFormat =  br.ReadUInt64();
                    header.Format = (PixelFormat)mFormat;
                    header.ColorSpace = br.ReadUInt32();
                    header.ChannelType = br.ReadUInt32();
                    header.Height = br.ReadUInt32();
                    header.Width = br.ReadUInt32();
                    header.Depth = br.ReadUInt32();
                    header.SurfaceNum = br.ReadUInt32();
                    header.FacesNum = br.ReadUInt32();
                    header.MipMapCount = br.ReadUInt32();
                    header.MetaDataSize = br.ReadUInt32();
                    br.BaseStream.Seek(header.MetaDataSize, SeekOrigin.Current);//skip header
                    bpp = PVRTGetBitsPerPixel(header.Format);
                    DataPosition = br.BaseStream.Position;
                    this.mainTexData = br.ReadBytes((int)(header.Width * header.Height * bpp / 8));


                }
            }
        }

        public byte[] GetPixelBytes()
        {
            byte[] output = new byte[header.Width * header.Height  * 4];
            byte[] input = this.mainTexData;
            uint width = header.Width;
            uint height = header.Height;
            int pos = 0;
            int outPos = 0;
            /*
            switch (this.header.Format)
            {
                case PixelFormat.RGBA4444:
                    pos = 0;
                    outPos = 0;
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            var v0 = input[pos];
                            var v1 = input[pos + 1];
                            // 4bit little endian {B, G}, {R, A }
                            // 16BIT位图 每个颜色占4bit 比如 一个 hex F1F2 > 0xF2F1
                            // B, G, R, A = 01, 0F, 02 ,0F
                            // 则 A , R, ,G, B = 0F, 02, 0F ,01
                            // 转换回RGBA8888 则是 02 0F 01 0F
                            var fA = v0 & 0xF0 >> 4; //低四位
                            var fR = (v0 & 0xF0) >> 4;//高四位
                            var fG = v1 & 0xF0 >> 4;//低四位
                            var fB = (v1 & 0xF0) >> 4;//高四位
                            /*var fA = v0 & 0xF0 >> 4;
                            var fB = (v0 & 0xF0) >> 4;
                            var fG = v1 & 0xF0 >> 4;
                            var fR = (v1 & 0xF0) >> 4;


                            fA = (fA * 255 + 7) / 15;
                            fR = (fR * 255 + 7) / 15;
                            fG = (fG * 255 + 7) / 15;
                            fB = (fB * 255 + 7) / 15;

                            output[outPos] = (byte)fR;
                            output[outPos + 1] = (byte)fG;
                            output[outPos + 2] = (byte)fB;
                            output[outPos + 3] = (byte)fA;

                            pos += (2);
                            outPos += 4;

                        }
                    }

                    break;
                default:
                    PVRTexLibNET.PixelFormat pFormat = (PVRTexLibNET.PixelFormat)this.header.Format;
                    using (var pvrTexture = PVRTexLibNET.PVRTexture.CreateTexture(input, (uint)width, (uint)height, 1, pFormat, false, VariableType.UnsignedByte, ColourSpace.sRGB))
                    {
                        pvrTexture.Transcode(PVRTexLibNET.PixelFormat.RGBA8888, VariableType.UnsignedByte, ColourSpace.sRGB, CompressorQuality.PVRTCNormal, false);
                        var texDataSize = pvrTexture.GetTextureDataSize(0);
                        var texData = new byte[texDataSize];
                        pvrTexture.GetTextureData(texData, texDataSize);
                        output = texData;

                    }
                    break;
            }
            */
            PVRTexLibNET.PixelFormat pFormat = (PVRTexLibNET.PixelFormat)this.header.Format;
            using (var pvrTexture = PVRTexLibNET.PVRTexture.CreateTexture(input, (uint)width, (uint)height, 1, pFormat, false, VariableType.UnsignedByte, ColourSpace.sRGB))
            {
                pvrTexture.Transcode(PVRTexLibNET.PixelFormat.RGBA8888, VariableType.UnsignedByte, ColourSpace.sRGB, CompressorQuality.PVRTCNormal, false);
                var texDataSize = pvrTexture.GetTextureDataSize(0);
                var texData = new byte[texDataSize];
                pvrTexture.GetTextureData(texData, texDataSize);
                output = texData;

            }

            return output;
        }

        public byte[] SetPixelBytes(byte[] sourceData)
        {
            byte[] output = new byte[header.Width * header.Height * bpp / 8];
            using (var pvrTexture = PVRTexLibNET.PVRTexture.CreateTexture(sourceData, (uint)header.Width, (uint)header.Height , 1, PVRTexLibNET.PixelFormat.RGBA8888, true, VariableType.UnsignedByte, ColourSpace.sRGB))
            {
                bool bDoDither = true;
                pvrTexture.Transcode((PVRTexLibNET.PixelFormat)this.header.Format, VariableType.UnsignedByte, ColourSpace.sRGB, CompressorQuality.PVRTCNormal, bDoDither);
                var texDataSize = pvrTexture.GetTextureDataSize(0);
                var texData = new byte[texDataSize];
                pvrTexture.GetTextureData(texData, texDataSize);
                output = texData;
            }

            return output;
        }

        public struct Header
        {
            // (0x34)52 bytes header 
            public UInt32 Version; // 0x03525650 little endian , 0x50565203 big endian

            public UInt32 Flags; // 0 = no flag , 0x2 = Pre-multiplied
            public PixelFormat Format; //8 bytes
            public UInt32 ColorSpace;
            public UInt32 ChannelType;
            
            public UInt32 Height;
            public UInt32 Width;
            public UInt32 Depth;
            public UInt32 SurfaceNum;
            public UInt32 FacesNum;

            public UInt32 MipMapCount;
            public UInt32 MetaDataSize;
            
        }



        public enum PixelFormat : ulong
        {
            PVRTCI_2bpp_RGB = 0,
            PVRTCI_2bpp_RGBA = 1,
            PVRTCI_4bpp_RGB = 2,
            PVRTCI_4bpp_RGBA = 3,
            PVRTCII_2bpp = 4,
            PVRTCII_4bpp = 5,
            ETC1 = 6,
            BC1 = 7,
            DXT1 = 7,
            DXT2 = 8,
            BC2 = 9,
            DXT3 = 9,
            DXT4 = 10,
            BC3 = 11,
            DXT5 = 11,
            BC4 = 12,
            BC5 = 13,
            BC6 = 14,
            BC7 = 15,
            UYVY = 16,
            YUY2 = 17,
            BW1bpp = 18,
            SharedExponentR9G9B9E5 = 19,
            RGBG8888 = 20,
            GRGB8888 = 21,
            ETC2_RGB = 22,
            ETC2_RGBA = 23,
            ETC2_RGB_A1 = 24,
            EAC_R11 = 25,
            EAC_RG11 = 26,
            RGB565 = 0x0005060500626772,
            RGBA4444 = 0x0404040461626772,
            RGBA8888 = 0x0808080861626772,
        }

        private UInt32 PVRTGetBitsPerPixel(PixelFormat pixelFormat)
        {
            UInt32 result = 0;
            switch (pixelFormat)
            {
                case PixelFormat.PVRTCI_2bpp_RGB:
                case PixelFormat.PVRTCI_2bpp_RGBA:
                case PixelFormat.PVRTCII_2bpp:
                    result = 2;
                    break;
                case PixelFormat.PVRTCI_4bpp_RGB:
                case PixelFormat.PVRTCI_4bpp_RGBA:
                case PixelFormat.PVRTCII_4bpp:
                case PixelFormat.ETC1:
                case PixelFormat.EAC_R11:
                case PixelFormat.ETC2_RGB:
                case PixelFormat.ETC2_RGB_A1:
                case PixelFormat.DXT1:
                case PixelFormat.BC4:
                    result = 4;
                    break;
                case PixelFormat.DXT2:
                case PixelFormat.DXT3:
                case PixelFormat.DXT4:
                case PixelFormat.DXT5:
                case PixelFormat.BC5:
                case PixelFormat.EAC_RG11:
                case PixelFormat.ETC2_RGBA:
                    return 8;
                case PixelFormat.RGBA4444:
                    return 16;
                case PixelFormat.RGB565:
                    return 24;
                case PixelFormat.RGBA8888:
                    return 32;
                case PixelFormat.YUY2:
                case PixelFormat.UYVY:
                case PixelFormat.RGBG8888:
                case PixelFormat.GRGB8888:
                    return 16;
                case PixelFormat.SharedExponentR9G9B9E5:
                    return 32;

            }
            return result;
        }

    }

    
}
