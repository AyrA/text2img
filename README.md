# text2img
Converts text files to images

## How to use

    text2img <infile> [/c:codepage] [outfile]
    
    infile     -  Text file to read contents from
    /c         -  Codepage to use. Specify /L to list all available codepages. Can use ID or name
    outfile    -  Output file. If not specified uses infile and swaps extension for ".png"

Windows GDI+ works with Int16 somewhere internally, so the maximum image dimensions is 32767*32767 pixels.
This application uses a 32bpp image in memory that would result in about 4 GB of ram for the image.

## Additional settings.

The console application doesn't supports additional arguments, but the image and text components do.
You can for example change the font and colors.

## Use as a service

You only need `Tools.cs`, `ImageTools.cs` and `TextTools.cs`.
