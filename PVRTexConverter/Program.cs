using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PVRTexConverter.Formats;
using System.IO;
using ImageMagick;

namespace PVRTexConverter
{
    public class AppMsg
    {
        public static string msg =
            "======Support Format====\n" +
            "PowerVR Textures *.pvr\n" +
            "========================\n" +
            " Usage:\n" +
            "-I -info :show texture info only\n" +
            "-d -dump :dump texture\n" +
            "-c -compress :compress texture\n" +
            "-i -input <path> :input name\n" +
            "-o -output <path> :output png name\n" +
            "\n";
    }
    class Program
    {
        
        [DllImport("user32.dll", EntryPoint = "MessageBox")]
        public static extern int MsgBox(IntPtr hwnd, string text, string caption, uint type);
        public static void ShowMsgBox(string msg)
        {
            MsgBox(IntPtr.Zero, msg, "PvrTexConverter", 1);
        }
        static void AddEnvironmentPaths()
        {
            System.Environment.SetEnvironmentVariable("PATH", System.IO.Path.Combine(Environment.CurrentDirectory, @"Library\") + ";"
                                                            , EnvironmentVariableTarget.Process);
        }
        private static void ShowArgsMsg()
        {

            Console.WriteLine(AppMsg.msg);
        }

        private static void PNG2Texture(string input_name, string output_name)
        {
            byte[] dstTex = File.ReadAllBytes(output_name);
            PVR texture = new PVR(dstTex);
            if (texture.isTexture == false)
            {
                return;
            }
            ImageMagick.MagickImage im = new MagickImage(input_name);
            if ((im.Width != texture.header.Width) || im.Height != texture.header.Height)
            {
                Console.WriteLine("Error: texture is {0} x {1} ,but png bitmap is {2} x {3}.Exit.",
                                    texture.header.Width, texture.header.Height,
                                    im.Width, im.Height);
                return;
            }
            byte[] sourceData = im.GetPixels().ToByteArray(0, 0, im.Width, im.Height, "RGBA");
            byte[] outputData;
            Console.WriteLine("Reading:{0}\n Width:{1}\n Height:{2}\n Format:{3}\n", input_name, im.Width, im.Height, texture.header.Format.ToString());
            Console.WriteLine("Converting...");
            outputData = texture.SetPixelBytes(sourceData);
            if (texture.header.MipMapCount > 1)
            {
                Console.WriteLine("Generating Mipmap...");
                for (var m = 0; m <= 3; m++)
                {

                    im.AdaptiveResize(im.Width / 2, im.Height / 2);
                    Console.WriteLine("Generating ...{0}x{1}", im.Width, im.Height);
                    sourceData = im.GetPixels().ToByteArray(0, 0, im.Width, im.Height, "RGBA");
                    byte[] dst;
                    dst = texture.SetPixelBytes(sourceData);
                    outputData = outputData.Concat(dst).ToArray();
                }

            }
            if (outputData != null)
            {
                if (File.Exists(output_name))
                {
                    Console.WriteLine("Writing...{0}", output_name);
                    using (FileStream fs = File.Open(output_name, FileMode.Open, FileAccess.ReadWrite))
                    {
                        fs.Seek(texture.DataPosition, SeekOrigin.Begin);
                        fs.Write(outputData, 0, outputData.Length);
                        fs.Flush();
                    }
                    Console.WriteLine("File Created...");
                }
                else
                {
                    Console.WriteLine("Error: file {0} not found", output_name);
                }
            }
        }

        private static void Texture2PNG(string input_name, string output_name)
        {
            byte[] input = File.ReadAllBytes(input_name);
            PVR texture = new PVR(input);
            if (texture.isTexture == false)
            {
                return;
            }
            PVR.Header header = texture.header;
            Console.WriteLine("Reading: {0}\n Width: {1}\n Height: {2}\n Format: {3}\n ColorSpace: {4}\n ChannelType: {5}\n " +
                                "Depth: {6}\n Mipmap: {7}\n Data Offset: {8:X8}",
                                Path.GetFileName(input_name) ,
                                header.Width,
                                header.Height,
                                header.Format.ToString(),
                                header.ColorSpace,
                                header.ChannelType,
                                header.Depth,
                                header.MipMapCount,
                                texture.DataPosition);
            MagickReadSettings settings = new MagickReadSettings();
            settings.Format = MagickFormat.Rgba;
            settings.Width = (int)header.Width;
            settings.Height = (int)header.Height;

            ImageMagick.MagickImage im = new MagickImage(texture.GetPixelBytes(), settings);
            im.ToBitmap().Save(output_name);


        }

        static void Main(string[] args)
        {
            string filename = null;
            string outputName = null;
            bool bDecompress = false;
            bool bCompress = false;
            bool bShowInfo = false;
            bool bShowHelp = false;
            if (args.Length == 0)
            {
                ShowArgsMsg();
                Program.ShowMsgBox(string.Format("Error: no args \n  Please use this program in console!\n"));

                return;
            }
            #region check args
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith("-"))
                    {
                        switch (args[i].TrimStart('-'))
                        {
                            case "h":
                            case "help":
                                bShowHelp = true;
                                break;
                            case "I":
                            case "info":
                                bShowInfo = true;
                                break;
                            case "d":
                            case "dump":
                                bDecompress = true;
                                break;
                            case "o":
                            case "output":
                                outputName = args[++i];
                                break;
                            case "c":
                            case "compress":
                                bCompress = true;
                                break;
                            case "i":
                            case "input":
                                filename = args[++i];
                                break;

                        }
                    }
                }
            }
            #endregion
            #region show help
            if (bShowHelp)
            {
                ShowArgsMsg();
                return;
            }

            #endregion

            if (bShowInfo && (filename != null))
            {
                try
                {
                    PVR texture = new PVR(File.ReadAllBytes(filename));
                    if (texture.isTexture == false)
                    {
                        return;
                    }
                    PVR.Header header = texture.header;
                    Console.WriteLine("Reading: {0}\n Width: {1}\n Height: {2}\n Format: {3}\n ColorSpace: {4}\n ChannelType: {5}\n " +
                                        "Depth: {6}\n Mipmap: {7}\n Data Offset: {8:X8}",
                                        filename,
                                        header.Width,
                                        header.Height,
                                        header.Format.ToString(),
                                        header.ColorSpace,
                                        header.ChannelType,
                                        header.Depth,
                                        header.MipMapCount,
                                        texture.DataPosition);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
                return;
            }
            if (filename == outputName)
            {
                Console.WriteLine("Error: can't overwrite input file");
                return;
            }
            if ((filename != null) && (outputName != null))
            {

                    if (bDecompress) Texture2PNG(filename, outputName);
                    if (bCompress) PNG2Texture(filename, outputName);
                
            }
#if DEBUG
            Console.ReadKey();
#endif
        }
    }
}
