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

        Console.Write("Enter your Server nickname: ");
        string nickname = Console.ReadLine() ?? "Server";

        Console.WriteLine("[SERVER] Started. Waiting for a client to connect...");

        TcpClient client = await server.AcceptTcpClientAsync();
        NetworkStream stream = client.GetStream();

        // Перший зчитаний рядок від клієнта — це його нікнейм
        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
        string clientNickname = Encoding.UTF8.GetString(buffer, 0, bytesRead);

        Console.WriteLine($"[SERVER] {clientNickname} ({client.Client.RemoteEndPoint}) connected to the chat.");

        using (client)
        {
            _ = ReceiveMessagesAsync(stream, clientNickname);

            while (true)
            {
                string? message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                    continue;

                if (message.ToLower() == "/exit")
                {
                    Console.WriteLine("Closing chat...");
                    break;
                }

                // Відправляємо повідомлення у форматі "Нікнейм: текст"
                string fullMessage = $"{nickname}: {message}";
                byte[] data = Encoding.UTF8.GetBytes(fullMessage);
                await stream.WriteAsync(data, 0, data.Length);
            }
        }
    }

    private static async Task ReceiveMessagesAsync(NetworkStream stream, string clientNickname)
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                {
                    Console.WriteLine($"\n[SERVER] {clientNickname} left the chat.");
                    break;
                }

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"\n{message}");
            }
        }
        catch (Exception)
        {
            Console.WriteLine($"\n[SERVER] Connection with {clientNickname} was lost.");
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

        Console.Write("Enter your Client nickname: ");
        string nickname = Console.ReadLine() ?? "Client";

        try
        {
            await client.ConnectAsync("127.0.0.1", 5000);
            NetworkStream stream = client.GetStream();

            // Клієнт одразу після підключення надсилає свій нікнейм серверу
            byte[] nicknameData = Encoding.UTF8.GetBytes(nickname);
            await stream.WriteAsync(nicknameData, 0, nicknameData.Length);

            Console.WriteLine("[CLIENT] Connected to the chat server. Type /exit to leave.");

            _ = ReceiveMessagesAsync(stream);

            while (true)
            {
                string? message = Console.ReadLine();

                if (string.IsNullOrEmpty(message))
                    continue;

                if (message.ToLower() == "/exit")
                {
                    Console.WriteLine("Closing chat...");
                    break;
                }

               
                string fullMessage = $"{nickname}: {message}";
                byte[] data = Encoding.UTF8.GetBytes(fullMessage);
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
                Console.WriteLine($"\n{response}");
            }
        }
        catch (Exception)
        {
            Console.WriteLine("\n[CLIENT] Connection with the server was lost.");
        }
    }
}