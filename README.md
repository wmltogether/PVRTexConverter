##Simple PVR (PowerVR Image Format) File Converter

Decompress .pvr files to .png & recompress them.

 PVR File Format Specification :
 See :[PVR File Format Specification](http://cdn.imgtec.com/sdk-documentation/PVR+File+Format.Specification.pdf)

##Usage:
 ```
 " Usage:\n" +
            "-I -info :show texture info only\n" +
            "-d -dump :dump texture\n" +
            "-c -compress :compress texture\n" +
            "-i -input <path> :input name\n" +
            "-o -output <path> :output png name\n" +
```

##Using librarys:

 PVRTexLibNET
 Magick.NET

##简易PVR 2 PNG转换工具
针对一些ios游戏中的pvr图片素材处理，这个小工具可以解压.pvr为常见的png图片并在修改后重新压缩.
这个工具不需要PVRTexTool依赖，方便简易，不破坏pvr结构也不会改变pvr图像的压缩格式。

##使用方法:
 ```
 " Usage:\n" +
            "-I -info :只显示pvr的图像信息，不解压图片\n" +
            "-d -dump :解压纹理到png\n" +
            "-c -compress :重新压缩纹理\n" +
            "-i -input <path> :输入路径\n" +
            "-o -output <path> :输出路径\n" +
```

##本工具使用以下开源库:

 PVRTexLibNET
 Magick.NET

