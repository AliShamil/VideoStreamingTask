﻿using System.Net.Sockets;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.IO.Compression;

var server = new UdpClient(45678);


while (true)
{
    var result = await server.ReceiveAsync();

    await Task.Run(() =>
     {
         var remoteEP = result.RemoteEndPoint;
         while (true)
         {
             var screenshot = GetScreenshot();
             var bytes = ImageToByteArray(screenshot);

             var chunk = bytes.Chunk(ushort.MaxValue - 29);


             foreach (var a in chunk)
             {
                 var s = Compress(a);
                 server.SendAsync(s, s.Length, remoteEP);
             }
         }
     });
    Console.WriteLine($"Received request from {result.RemoteEndPoint}");


}

Bitmap GetScreenshot()
{
    Bitmap screenshot;
    screenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
        Screen.PrimaryScreen.Bounds.Height);

    Graphics graphics = Graphics.FromImage(screenshot);
    graphics.CopyFromScreen(0, 0, 0, 0, screenshot.Size);

    return screenshot;
}

byte[] ImageToByteArray(System.Drawing.Image image)
{
    using (MemoryStream stream = new MemoryStream())
    {
        image.Save(stream, ImageFormat.Jpeg);
        return stream.ToArray();
    }
}

byte[] Compress(byte[] raw)
{
    using (MemoryStream memory = new MemoryStream())
    {
        using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
        {
            gzip.Write(raw, 0, raw.Length);
        }
        return memory.ToArray();
    }
}