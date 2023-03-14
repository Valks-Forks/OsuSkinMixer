using System;
using Godot;
using OsuSkinMixer.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using File = System.IO.File;

namespace OsuSkinMixer.Statics;

public static class Tools
{
    public static void ShellOpenFile(string path)
        => OS.ShellOpen(OS.GetName() == "macOS" ? $"file://{path}" : path);

    public static void TriggerOskImport(OsuSkin skin)
    {
        string oskDestPath = $"{Settings.SkinsFolderPath}/{skin.Name}.osk";
        Settings.Log($"Importing skin into game from '{oskDestPath}'");

        // osu! will handle the empty .osk (zip) file by switching the current skin to the skin with name `newSkinName`.
        File.WriteAllBytes(oskDestPath, new byte[] { 0x50, 0x4B, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
        ShellOpenFile(oskDestPath);
    }

    public static Rectangle GetContentRectFromImage(Image<Rgba32> image)
    {
        int width = image.Width;
        int height = image.Height;

        int minX = width;
        int minY = height;
        int maxX = 0;
        int maxY = 0;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Rgba32 pixel = image[x, y];

                if (pixel.A != 0)
                {
                    minX = Math.Min(minX, x);
                    minY = Math.Min(minY, y);
                    maxX = Math.Max(maxX, x);
                    maxY = Math.Max(maxY, y);
                }
            }
        }

        if (minX == width || minY == height || maxX == 0 || maxY == 0)
            return Rectangle.Empty;

        return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }
}