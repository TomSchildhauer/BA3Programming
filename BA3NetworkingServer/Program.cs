// This is the server for the CGL BA3 course on Networking for Games.
// It is a simple TCP server that listens on port 20000 for incoming connections.
// Its purpose is to demonstrate how to send and receive messages between a server and a client.
// Clients can connect to this server and will receive a leaderboard of the top 10 players.
// The server will also receive messages from the clients and change the leaderboard entries accordingly.
// The implementation is purposefully kept simple so that the students have a chance to "hack" it with their clients.


using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;

using System.Net;
using System.Net.Sockets;

namespace BA3NetworkingServer {

    class Program {

        private static List<TcpClient> clients = new List<TcpClient>();
        private static Dictionary<string, int> leaderboard = new Dictionary<string, int>();

        static void Main (string[] args) {
            
            // Create a new TCP server that listens on port 20000
            TcpListener server = new TcpListener(IPAddress.Any, 20000);  

            Console.WriteLine("Starting server. Listening on port 20000");

            // Maintain a list of all connected clients

            server.Start();  // this will start the server
            
            Console.WriteLine("Server started");

            // Endless loop. This loop is used to wait for connections
            while (true) {

                // If there is a pending connection, accept it
                if (server.Pending()) {

                    Console.WriteLine("New connection pending!");

                    // Accept the connection
                    TcpClient client = server.AcceptTcpClient();

                    // Add the client to the list
                    clients.Add(client);

                    // Inform the server console that a new client has connected
                    Console.WriteLine("New client connected: " + client.Client.RemoteEndPoint?.ToString());

                    // Send the leaderboard to the client
                    Welcome(client);
                }

                // Keep a list of clients that have disconnected
                List<TcpClient> disconnectedClients = new List<TcpClient>();

                // Check if there are any clients that have sent messages
                foreach (TcpClient client in clients) {

                    // If there is a message, read it and process it
                    if (client.Available > 0) {

                        Console.WriteLine("New message from client: " + client.Client.RemoteEndPoint?.ToString() + "\nReading...");

                        // Read the message
                        string message = ReadMessage(client);

                        Console.WriteLine("Message: " + message + "\nProcessing...");

                        // Process the message
                        if (ProcessMessage(message)) {

                            Console.WriteLine("Message processed successfully. Sending leaderboard to all clients...");

                            // Send the updated leaderboard to all clients
                            foreach (TcpClient c in clients)
                                SendLeaderboard(c);

                        } else {

                            Console.WriteLine("Message could not be processed. Sending error message to client...");

                            // If the message could not be processed, send the client a message
                            SendMessage(client, "Invalid message. Please send your name and score in the following format: name,score\nExample: Lucy,100");

                        }

                    } else if (!client.Connected) {

                        // If the client is not connected, add it to the list of disconnected clients
                        disconnectedClients.Add(client);

                        // Inform the server console that a client has disconnected
                        Console.WriteLine("Client disconnected: " + client.Client.RemoteEndPoint?.ToString());
                    }
                }

                // Remove all disconnected clients from the list
                foreach (TcpClient client in disconnectedClients)
                    clients.Remove(client);
            }
        }

        // This method reads and processes a message from a client
        static string ReadMessage (TcpClient client) {

            // Get the stream from the client
            NetworkStream stream = client.GetStream();

            // Create a buffer to store the message
            byte[] buffer = new byte[client.Available];

            // Read the message from the stream
            stream.Read(buffer, 0, buffer.Length);

            // Convert the message to a string
            string message = Encoding.UTF8.GetString(buffer);

            // Return the message
            return message;
        }


        // This method processes a message from a client
        static bool ProcessMessage (string message) {

            try {

            
                // Split the message into its parts
                string[] parts = message.Split(',');

                // Check if the message is a valid message
                if (parts.Length == 2) {

                    // Get the name and score from the message
                    string name = parts[0];
                    int score = int.Parse(parts[1]);

                    Console.WriteLine("Name: " + name + "  |  Score: " + score + "\nUpdating leaderboard...");

                    // Update the leaderboard with the new score
                    leaderboard[name] = score;
                    
                    return true;
                }

                return false;

            } catch (Exception e) {
                Console.WriteLine("Exception caught: " + e.Message);
                return false;
            }
        }

        // This method sends the leaderboard to a client
        static void SendLeaderboard (TcpClient client) {

            // Check if the client is connected and if there are any entries in the leaderboard
            if (leaderboard.Count == 0) {
                Console.WriteLine("Leaderboard is empty. Not sending to client.");
                return;
            }

            // Create a list of the leaderboard entries from the dictionary
            List<KeyValuePair<string, int>> leaderboardList = leaderboard.ToList(); 

            // Sort the leaderboard by score
            leaderboardList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            // Take the top 10 entries
            leaderboardList = leaderboardList.Take(10).ToList();

            // Create a string that contains the leaderboard
            string message = " ==== LEADERBOARD ==== \n";
            foreach (KeyValuePair<string, int> entry in leaderboardList)
                message += entry.Key + "  -  " + entry.Value + "\n";

            // Send the leaderboard to the client
            SendMessage(client, message);
        }

        // This method sends a welcome message to a client
        static void Welcome (TcpClient client) {

            // Create a string that contains the welcome message
            string message = "Welcome to the leaderboard server!\n";
            message += "Please send your name and score in the following format: name,score\n";
            message += "Example: Lucy,100\n\n";
            message += "The leaderboard will be updated accordingly. The leaderboard will be sent to all clients whenever it is updated.\n";
            message += "Have fun!";

            SendMessage(client, message);
        }

        static bool SendMessage (TcpClient client, string message) {

            try {
            
                // Get the stream from the client
                NetworkStream stream = client.GetStream();

                // Convert the message to a byte array
                byte[] buffer = Encoding.UTF8.GetBytes(message);

                // Send the message to the client
                stream.Write(buffer, 0, buffer.Length);

                return true;
            
            } catch (Exception e) {
                Console.WriteLine("Exception caught: " + e.Message);
                return false;
            }
        }

    }
}