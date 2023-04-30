using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Dismobi;

public partial class MainPage : ContentPage
{

    UnicodeEncoding unicode_encoding = new UnicodeEncoding();

    IPEndPoint iep = new IPEndPoint(IPAddress.Parse("192.168.0.126"), 9999);
    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

    public MainPage()
	{

		InitializeComponent();
    }

    private async void Connect_Click(object sender, EventArgs e)
    {

        if (client.Connected != true)
            client.Connect(iep);

        await Task.Run(() => DoSomethingLong());
    }

    private async void DoSomethingLong()
    {

        byte[] buffer = new byte[10000000];
        int offset = 0;

        while (true)
        {

            int bytes_received = client.Receive(buffer, offset, buffer.Length - offset,
                SocketFlags.None);

            Console.WriteLine($"bytes_received {bytes_received}");

            MemoryStream memory_stream = new MemoryStream(buffer, 0, offset + bytes_received);
            {

                var expected_block_length = readInt32(memory_stream);

                Console.WriteLine($"expected_block_length {expected_block_length}");

                // sa verfic IMEDIAT AICI daca sunt suficiente date !!!!!!!!!
                if (offset + bytes_received < expected_block_length)
                {

                    offset += bytes_received;

                    Console.WriteLine($"offset {offset}");

                    continue;
                }

                var text_top_left = readString(memory_stream, unicode_encoding);

                var text_bottom_left = readString(memory_stream, unicode_encoding);

                var text_bottom_center = readString(memory_stream, unicode_encoding);

                var text_bottom_right = readString(memory_stream, unicode_encoding);

                // n_reds
                var reds_x = new int[memory_stream.ReadByte()];
                var reds_y = new int[reds_x.Length];

                for (int i = 0; i < reds_x.Length; i++)
                {

                    reds_x[i] = readInt32(memory_stream);
                    reds_y[i] = readInt32(memory_stream);
                }

                // n_blues
                var blues_x = new int[memory_stream.ReadByte()];
                var blues_y = new int[blues_x.Length];

                for (int i = 0; i < blues_x.Length; i++)
                {

                    blues_x[i] = readInt32(memory_stream);
                    blues_y[i] = readInt32(memory_stream);
                }

                // file length
                var file_buffer = new byte[ readInt32(memory_stream) ];

                memory_stream.Read(file_buffer);

                var photo_stream = ImageSource.FromStream(() => { return new MemoryStream(file_buffer); });

                var remaining = offset + bytes_received - expected_block_length;
                Array.Copy(buffer, expected_block_length, buffer, 0, remaining);
                offset = remaining;

                Console.WriteLine($"texts { text_top_left }, { text_bottom_left }, { text_bottom_center }, { text_bottom_right }");

                MainThread.BeginInvokeOnMainThread( () => {

                    labelTopLeft.Text = text_top_left;
                    labelBottomLeft.Text = text_bottom_left;
                    labelBottomCenter.Text = text_bottom_center;
                    labelBottomRight.Text = text_bottom_right;

                    imageFrame.Source = photo_stream;

                } );
                                
            }





            // move unread data to the beginning of the buffer and update offset
            

            /*var img = Bitmap.FromStream(stream);
            pictureBox1.Image = img;*/
        }
    }

    public int readInt32(MemoryStream memory_stream)
    {

        int number =
            (int)(memory_stream.ReadByte() << 24) |
            (int)(memory_stream.ReadByte() << 16) |
            (int)(memory_stream.ReadByte() << 8) |
            (int)(memory_stream.ReadByte());

        return number;
    }

    public string readString(MemoryStream memory_stream, UnicodeEncoding encoding)
    {

        int length = memory_stream.ReadByte();

        var bytes = new byte[length];

        memory_stream.Read(bytes, 0, length);

        var text = encoding.GetString(bytes);

        return text;
    }
}


