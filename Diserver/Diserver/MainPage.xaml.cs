using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics.Platform;

namespace Diserver;

public partial class MainPage : ContentPage
{

    UnicodeEncoding unicode_encoding = new UnicodeEncoding();

    double frequency = 31, noise = 59, flow = 50, yellow_flow = 95, pressure = 24, af = 98;
    int x = 300;
    Socket client = null;

    public MainPage()
    {
        InitializeComponent();

        /*IDispatcherTimer timer;

        timer = Dispatcher.CreateTimer();
        timer.Interval = TimeSpan.FromMilliseconds(1000);
        timer.Tick += (s, e) =>
        {
            labelBottomCenter.Text = DateTime.Now.ToString();
        };
        timer.Start();*/

        //var xres = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();



        var frames = new byte[10][];

        int i;

        for (i = 0; i < frames.Length; i++)
        {

            Assembly assembly = GetType().GetTypeInfo().Assembly;
            using (Stream stream = assembly.GetManifestResourceStream(
                $"Diserver.Resources.Images.pipes{i}a.png"))
            {

                frames[i] = new byte[ stream.Length ];
                stream.Read(frames[i], 0, (int) stream.Length);
            }
        }

        IPEndPoint iep = new IPEndPoint(IPAddress.Parse("192.168.0.126"), 9999);
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        server.Bind(iep);
        server.Listen(100);
        Console.WriteLine("Waiting for client....");
        client = server.Accept();

        var random = new Random();

        i = 0;
        while ( true )
        {

            Thread.Sleep(50);

            /*Bitmap bmp = TakingScreenshotEx1(); // method for taking screenshot
            bmp.Save("1.jpeg", ImageFormat.Jpeg); // the name of the screenshot taken, just before 

            byte[] buffer = ReadImageFile("1.jpeg"); // read the saved image file 
            int v = client.Send(buffer, buffer.Length, SocketFlags.None); // send the image*/

            i = i == frames.Length - 1 ? 0 : i + 1;
            x = i == frames.Length - 1 ? 300 : x - 10;

            var text_top_left = $"{ (frequency + 2 * random.NextDouble() ):0.#} kHz | { (noise + 2 * random.NextDouble() ):0.#} dB";
            var text_bottom_left = $"{ (flow + 3 * random.NextDouble()) :0.} L/h |";
            var text_bottom_center = $"{ (yellow_flow + 6 * random.NextDouble()):0.} L/h |";
            var text_bottom_right = $"{ (pressure + 2 * random.NextDouble() ):0.} bar\nAF { ( af + 7 * random.NextDouble() ):0.#} cm";
            //length,text1,text2,text3,text4,n_reds,reds1x,reds1y,redsnx,redsny,
            //n_blues,blues1x,blues1y,bluesnx,bluesny,length_in_,jpeg
            //client.Send();

            using (MemoryStream memory_stream = new MemoryStream(1000 + frames[i].Length))
            {

                writeString(memory_stream, text_top_left, unicode_encoding);

                writeString(memory_stream, text_bottom_left, unicode_encoding);

                writeString(memory_stream, text_bottom_center, unicode_encoding);

                writeString(memory_stream, text_bottom_right, unicode_encoding);

                // n_reds
                memory_stream.WriteByte(1);

                writeInt32(memory_stream, x - 50);
                writeInt32(memory_stream, 100);

                // n_blues
                memory_stream.WriteByte(1);

                writeInt32(memory_stream, x + 100);
                writeInt32(memory_stream, 400);

                // file length
                writeInt32(memory_stream, frames[i].Length);

                memory_stream.Write(frames[i], 0, frames[i].Length);

                using (MemoryStream memory_stream_length = new MemoryStream(4))
                {

                    writeInt32( memory_stream_length, (int) memory_stream.Length + 4 );

                    int v1 = client.Send(memory_stream_length.ToArray(), 4, SocketFlags.None);
                }

                int v2 = client.Send(memory_stream.ToArray(), (int) memory_stream.Length, SocketFlags.None);

                Console.WriteLine($"texts { text_top_left }, { text_bottom_left }, { text_bottom_center }, { text_bottom_right }");
            }
        }
    }

    public void writeInt32(MemoryStream memory_stream, int number)
    {

        var b3 = (byte)((number & 0x7f000000) >> 24);
        memory_stream.WriteByte(b3);

        var b2 = (byte)((number &   0xff0000) >> 16);
        memory_stream.WriteByte(b2);

        var b1 = (byte)((number &     0xff00) >> 8);
        memory_stream.WriteByte(b1);

        var b0 = (byte)( number &       0xff);
        memory_stream.WriteByte(b0);
    }

    public void writeString(MemoryStream memory_stream, string text, UnicodeEncoding encoding)
    {

        var bytes = encoding.GetBytes(text);

        memory_stream.WriteByte((byte)bytes.Length);

        memory_stream.Write(bytes, 0, bytes.Length);
    }
}


