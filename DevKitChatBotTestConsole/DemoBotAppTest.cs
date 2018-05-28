namespace DevKitChatBotTestConsole
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;

    public class DemoBotAppTest
    {
        /// <summary>
        /// A completed task
        /// </summary>
        private static readonly Task CompletedTask = Task.FromResult(true);

        /// <summary>
        /// Cancellation token used to stop sending the audio.
        /// </summary>
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        public async Task RunAsync()
        {
            using (ClientWebSocket webSocketClient = new ClientWebSocket())
            {
                Uri serverUri = new Uri($"ws://demo-bot-app.azurewebsites.net/chat?nickName={Guid.NewGuid().ToString()}");

                // Connect to WebSocket server
                try
                {
                    await webSocketClient.ConnectAsync(serverUri, CancellationToken.None);
                    Console.WriteLine($"WebSocket Connect succeeded.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"WebSocket Connect filed: {e.InnerException.ToString()}");
                    return;
                }

                List<byte> totalReceived = new List<byte>();
                ArraySegment<byte> receivedBuffer = new ArraySegment<byte>(new byte[1024 * 10]);
                WebSocketReceiveResult receiveResult;

                if (webSocketClient.State == WebSocketState.Open)
                {
                    /*
                    // Send text message to server
                    string sendMsg = "Could you play some music?";
                    Console.WriteLine($"Command> {sendMsg}");

                    ArraySegment<byte> bytesToSend = new ArraySegment<byte>(Encoding.UTF8.GetBytes(sendMsg));
                    await webSocketClient.SendAsync(bytesToSend, WebSocketMessageType.Text, true, CancellationToken.None);

                    // Receive text message from server
                    receiveResult = await webSocketClient.ReceiveAsync(receivedBuffer, CancellationToken.None);
                    Console.WriteLine(Encoding.UTF8.GetString(receivedBuffer.Array, 0, receiveResult.Count));
                    */

                    // Send binary message to server
                    byte[] bytes = File.ReadAllBytes(@"C:\IoT\Voice\devkitAudio.pcm");
                    await SendBinaryAsync(bytes, webSocketClient);

                    // receive binary from server
                    totalReceived.Clear();
                    receiveResult = await webSocketClient.ReceiveAsync(receivedBuffer, CancellationToken.None);
                    MergeFrameContent(totalReceived, receivedBuffer.Array, receiveResult.Count);

                    while (webSocketClient.State == WebSocketState.Open && !receiveResult.EndOfMessage)
                    {
                        receiveResult = await webSocketClient.ReceiveAsync(receivedBuffer, CancellationToken.None);
                        MergeFrameContent(totalReceived, receivedBuffer.Array, receiveResult.Count);
                    }

                    Utility.PlayAudio(new MemoryStream(totalReceived.ToArray()));

                    Thread.Sleep(1000);
                }
            }
        }

        private async Task SendBinaryAsync(byte[] bytes, ClientWebSocket client)
        {
            if (client == null || client.State != WebSocketState.Open)
            {
                throw new InvalidOperationException("Socket is not available");
            }

            const int FrameBytesCount = 10 * 1024;
            int sentBytes = 0;
            while (sentBytes < bytes.Length)
            {
                int remainingBytes = bytes.Length - sentBytes;
                bool isEndOfMessage = remainingBytes > FrameBytesCount ? false : true;

                await client.SendAsync(
                    new ArraySegment<byte>(bytes, sentBytes, remainingBytes > FrameBytesCount ? FrameBytesCount : remainingBytes),
                    WebSocketMessageType.Binary,
                    isEndOfMessage,
                    CancellationToken.None);

                sentBytes += remainingBytes > FrameBytesCount ? FrameBytesCount : remainingBytes;

                Thread.Sleep(50);
            }
        }

        private void MergeFrameContent(List<byte> destBuffer, byte[] buffer, long count)
        {
            count = count < buffer.Length ? count : buffer.Length;

            if (count == buffer.Length)
            {
                destBuffer.AddRange(buffer);
            }
            else
            {
                var frameBuffer = new byte[count];
                Array.Copy(buffer, frameBuffer, count);

                destBuffer.AddRange(frameBuffer);
            }
        }
    }
}
