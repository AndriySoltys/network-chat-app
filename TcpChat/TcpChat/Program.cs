
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

// ==========================================
// PROGRAM MAIN 

Console.OutputEncoding = Encoding.UTF8;

Console.WriteLine("1 - Server");
Console.WriteLine("2 - Client");

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
// SERVER CLASS

public class Server
{
    public static async Task Start()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();

        Console.WriteLine("[SERVER] Started. Waiting for a client to connect...");

        TcpClient client = await server.AcceptTcpClientAsync();
        Console.WriteLine($"[SERVER] Client {client.Client.RemoteEndPoint} connected to the chat.");

        using (client)
        {
            NetworkStream stream = client.GetStream();

            _ = ReceiveMessagesAsync(stream);

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

    private static async Task ReceiveMessagesAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine("\n[SERVER] Client left the chat.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"\n[CLIENT]: {message}");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("\n[SERVER] Connection with the client was lost.");
        }
    }
}


// ==========================================
// CLIENT CLASS

public class Client
{
    public static async Task Start()
    {
        using TcpClient client = new TcpClient();

        try
        {
            await client.ConnectAsync("127.0.0.1", 5000);
            Console.WriteLine("[CLIENT] Connected to the chat server. You can type now:");

            NetworkStream stream = client.GetStream();

            _ = ReceiveMessagesAsync(stream);

            while (true)
            {
                string? message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                    continue;

                byte[] data = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(data, 0, data.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CLIENT ERROR] {ex.Message}");
        }
    }

    private static async Task ReceiveMessagesAsync(NetworkStream stream)
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine("\n[CLIENT] Server closed the chat.");
                    break;
                }

                string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"\n[SERVER]: {response}");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("\n[CLIENT] Connection with the server was lost.");
        }
    }
}