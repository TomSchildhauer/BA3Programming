// This is the client for the CGL BA3 course on Networking for Games.
// It is a simple TCP client that connects to a server and sends and receives messages.
// Its purpose is to demonstrate how to send and receive messages between a server and a client.

using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;

using System.Net;
using System.Net.Sockets;

namespace BA3NetworkingClient {

    class Program {

        private static string host = "localhost";
        private static int port = 20000;
            
        static void Main (string[] args) {

            // Create a new TCP client
            TcpClient client = new TcpClient();

            // Read the host IP address from the console
            Console.WriteLine("Enter the host IP address [" + host + "]:");
            string? input = Console.ReadLine();
            
            // Default to host variable if no input
            if (input != null && input != "")
                host = input;
            
            // Read the port number from the console
            Console.WriteLine("Enter the port number [" + port + "]:");
            input = Console.ReadLine();

            // Default to port variable if no input
            if (input != null && input != "")
                port = int.Parse(input);
            
            Console.WriteLine("Connecting to " + host + ":" + port);

            // Connect to the server
            client.Connect(host, port);

            Console.WriteLine("Connected");

            // Endless loop. This loop is used to wait for messages from the server
            while (true) {

                // If there is a message, read it and process it
                if (client.Available > 0) {

                    // Read the message
                    string message = ReadMessage(client);

                    Console.WriteLine("\nMessage from server:\n" + message + "\n");
                }

                // Check if the user has entered a message
                if (Console.KeyAvailable) {

                    // Read the message
                    string? message = Console.ReadLine();

                    if (message == null)
                        continue;

                    Console.WriteLine("Sending message: " + message);

                    // Send the message
                    SendMessage(client, message);
                }
            }
        }
    
        // This method reads a message from the server
        private static string ReadMessage (TcpClient client) {

            // Create a new byte array with the size of the message
            byte[] buffer = new byte[client.Available];

            // Read the message into the buffer
            client.GetStream().Read(buffer, 0, buffer.Length);

            // Convert the message to a string
            string message = Encoding.UTF8.GetString(buffer);

            // Return the message
            return message;
        }

        // This method sends a message to the server
        private static void SendMessage (TcpClient client, string message) {

            // Convert the message to a byte array
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            // Send the message to the server
            client.GetStream().Write(buffer, 0, buffer.Length);
        }
    }
}

