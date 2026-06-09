using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using UnityEngine;

public class LocalWebSocketServer : MonoBehaviour
{
    [Header("Server Configurations")]
    public int port = 8080;
    
    private TcpListener tcpListener;
    private bool isRunning = false;
    
    // Thread-safe dictionary to track connected mobile player states
    public static ConcurrentDictionary<string, ControllerInput> PlayerInputs = new ConcurrentDictionary<string, ControllerInput>();

    [System.Serializable]
    public struct ControllerInput
    {
        public float x;
        public float y;
        public bool boost;
        public long lastUpdateTime;
    }

    void Start()
    {
        StartServer();
    }

    public void StartServer()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            isRunning = true;
            Debug.Log($"[Heist Server] WebSocket Server successfully launched on port: {port}");
            
            // Begin listening for mobile browser clients asynchronously
            Task.Run(() => ListenForClientsAsync());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Heist Server] Failed to bind server to port {port}: {ex.Message}");
        }
    }

    private async Task ListenForClientsAsync()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                // Hand off the connected phone client to its own async processing thread
                _ = Task.Run(() => HandleClientAsync(client));
            }
            catch
            {
                // Server closed or client dropped cleanly
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            string clientIp = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
            string playerKey = $"Player_{clientIp.Replace(".", "_")}";
            
            byte[] buffer = new byte[1024];
            
            while (isRunning && client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // Disconnected

                string rawMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                
                // --- Simple WebSocket Handshake Check ---
                if (rawMessage.Contains("GET / HTTP/1.1"))
                {
                    await PerformWebSocketHandshake(stream, rawMessage);
                    continue;
                }

                // Decode real-time control frames from the mobile controller app
                ParseWebSocketFrame(playerKey, buffer, bytesRead);
            }
            
            // Wipe input registration if player closes phone browser or disconnects
            PlayerInputs.TryRemove(playerKey, out _);
            Debug.Log($"[Heist Server] {playerKey} disconnected from match.");
        }
    }

    private async Task PerformWebSocketHandshake(NetworkStream stream, string rawHeaders)
    {
        string key = rawHeaders.Split(new[] { "Sec-WebSocket-Key: " }, StringSplitOptions.None)[1].Split('\r')[0].Trim();
        string acceptKey = Convert.ToBase64String(
            System.Security.Cryptography.SHA1.Create().ComputeHash(
                Encoding.UTF8.GetBytes(key + "258EAFA5-E914-47DA-95CA-C5ABDC427B43")
            )
        );

        string newline = "\r\n";
        string response = "HTTP/1.1 101 Switching Protocols" + newline +
                         "Upgrade: websocket" + newline +
                         "Connection: Upgrade" + newline +
                         $"Sec-WebSocket-Accept: {acceptKey}" + newline + newline;

        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
        await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    private void ParseWebSocketFrame(string playerKey, byte[] frameBuffer, int totalLength)
    {
        // Simple bitmask parsing to extract payload text from standard browser web sockets safely
        if (totalLength < 6) return; 

        bool isMasked = (frameBuffer[1] & 0x80) != 0;
        int payloadLen = frameBuffer[1] & 0x7F;
        
        int maskOffset = 2;
        if (payloadLen == 126) maskOffset = 4;
        else if (payloadLen == 127) maskOffset = 10;

        byte[] masks = new byte[4];
        if (isMasked)
        {
            Array.Copy(frameBuffer, maskOffset, masks, 0, 4);
            maskOffset += 4;
        }

        int actualPayloadDataLength = totalLength - maskOffset;
        byte[] decodedPayload = new byte[actualPayloadDataLength];

        for (int i = 0; i < actualPayloadDataLength; i++)
        {
            decodedPayload[i] = (byte)(frameBuffer[maskOffset + i] ^ (isMasked ? masks[i % 4] : 0));
        }

        string jsonString = Encoding.UTF8.GetString(decodedPayload).Trim();
        
        try
        {
            // Unpack payload fields safely directly to our player profile tracker
            ControllerInput rawInput = JsonUtility.FromJson<ControllerInput>(jsonString);
            rawInput.lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            PlayerInputs[playerKey] = rawInput;
        }
        catch
        {
            // Gracefully ignore structurally invalid JSON tracking frames
        }
    }

    void OnDestroy()
    {
        isRunning = false;
        tcpListener?.Stop();
    }
}
