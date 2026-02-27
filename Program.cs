using System;
using System.IO;
using System.Diagnostics;

class UFontExtractor
{
    static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("ufont to ttf Converter");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Made by Landmark");
        Console.WriteLine();
        Console.ResetColor();
        Console.WriteLine("↓Drag and drop the ufont file↓");

        if (args.Length == 0)
        {

            string inputPath = Console.ReadLine()?.Trim('"');

            if (string.IsNullOrWhiteSpace(inputPath) || !File.Exists(inputPath))
            {
                Console.WriteLine("指定されたファイルが存在しません。");
                Console.WriteLine("Enterキーで終了します...");
                Console.ReadLine();
                return;
            }

            args = new string[] { inputPath };
        }

        foreach (var inputPath in args)
        {
            if (!File.Exists(inputPath))
            {
                Console.WriteLine($"ファイルが見つかりません: {inputPath}");
                continue;
            }

            string outputPath = Path.ChangeExtension(inputPath, ".ttf");
            Console.WriteLine($"抽出処理中: {Path.GetFileName(inputPath)}");

            string result = ExtractTTF(inputPath, outputPath);
            Console.WriteLine(result);

            if (result.StartsWith("Success"))
            {
                OpenFolderAndSelectFile(outputPath);
            }
        }

        Console.WriteLine("完了！Enterキーで終了します。");
        Console.ReadLine();
    }

    public static string ExtractTTF(string inputPath, string outputPath)
    {
        byte[] data = File.ReadAllBytes(inputPath);

        int start = FindTTFHeader(data);
        if (start == -1)
            return "ヘッダーが見つかりませんでした。";

        int numTables = (data[start + 4] << 8) | data[start + 5];

        int maxOffset = 0;
        int tableOffset = start + 12;
        for (int i = 0; i < numTables; i++)
        {
            int entryOffset = tableOffset + i * 16;
            int offset = ReadInt32BE(data, entryOffset + 8);
            int length = ReadInt32BE(data, entryOffset + 12);
            int end = offset + length;
            if (end > maxOffset)
                maxOffset = end;
        }

        if (maxOffset + start > data.Length)
            return "フォントが破損している可能性があります。";

        byte[] ttfData = new byte[maxOffset];
        Array.Copy(data, start, ttfData, 0, maxOffset);
        File.WriteAllBytes(outputPath, ttfData);

        return $"抽出成功: {Path.GetFileName(outputPath)}";
    }

    private static int FindTTFHeader(byte[] data)
    {
        for (int i = 0; i < data.Length - 4; i++)
        {
            // TrueType
            if (data[i] == 0x00 && data[i + 1] == 0x01 && data[i + 2] == 0x00 && data[i + 3] == 0x00)
                return i;
            // Apple TrueType
            if (data[i] == 't' && data[i + 1] == 'r' && data[i + 2] == 'u' && data[i + 3] == 'e')
                return i;
            // OpenType (CFF)
            if (data[i] == 'O' && data[i + 1] == 'T' && data[i + 2] == 'T' && data[i + 3] == 'O')
                return i;
        }
        return -1;
    }

    private static int ReadInt32BE(byte[] data, int offset)
    {
        return (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
    }

    private static void OpenFolderAndSelectFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
        }
    }
}