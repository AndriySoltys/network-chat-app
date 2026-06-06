using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// ==========================================
//  Program

Console.WriteLine("1 - Сервер");
Console.WriteLine("2 - Клієнт");

string? choice = Console.ReadLine();

if (choice == "1")
{
    await Server.Start();
}
else
{
    await Client.Start();
}


// ==========================================
// Server

public class Server
{
    public static async Task Start()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();

        Console.WriteLine("Сервер запущено");

        TcpClient client = await server.AcceptTcpClientAsync();

        Console.WriteLine("Клієнт підключився");

        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];

        while (true)
        {
            int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            if (bytesRead == 0)
                break;

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            Console.WriteLine($"Отримано: {message}");
        }
    }
}


// ==========================================
// Client

public class Client
{
    public static async Task Start()
    {
        TcpClient client = new TcpClient();

        await client.ConnectAsync("127.0.0.1", 5000);

        Console.WriteLine("Підключено до сервера");

        NetworkStream stream = client.GetStream();

        while (true)
        {
            string? message = Console.ReadLine();

            if (string.IsNullOrEmpty(message))
                continue;

            byte[] data = Encoding.UTF8.GetBytes(message);

            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}