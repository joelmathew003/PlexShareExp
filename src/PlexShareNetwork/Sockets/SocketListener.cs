/// <author>Mohammad Umar Sultan</author>
/// <created>16/10/2022</created>
/// <summary>
/// This file contains the class definition of SocketListener.
/// </summary>

using PlexShareNetwork.Queues;
using PlexShareNetwork.Serialization;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace PlexShareNetwork.Sockets
{
	public class SocketListener
	{
		// max size of the send buffer
		private const int bufferSize = 1000000;
		// create the buffer
		private readonly byte[] buffer = new byte[bufferSize];

		// object to store the the received message, StringBuilder type is mutable while string type is not
		private readonly StringBuilder _receivedString = new();

        // the thread which will be running
        private readonly Thread _thread;
		// boolean to tell whether the thread is running or stopped
		private bool _threadRun;

		// variable to store the receive queue
		private readonly ReceivingQueue _receivingQueue;

		// variable to store the socket
		private readonly Socket _socket;

        /// <summary>
        /// It is the Constructor which initializes the queue and socket variables.
        /// </summary>
        /// <param name="receivingQueue"> The receiving queue. </param>
        /// <param name="socket"> The socket on which to listen. </param>
        public SocketListener(ReceivingQueue receivingQueue, TcpClient socket)
		{
            _receivingQueue = receivingQueue;
			_socket = socket.Client;
            _thread = new Thread(() => _socket.BeginReceive(buffer, 0, bufferSize, 0, ReceiveCallback, null));
		}

        /// <summary>
        /// This function starts the thread.
        /// </summary>
        /// <returns> void </returns>
        public void Start()
		{
            Trace.WriteLine("[Networking] SocketListener.Start() function called.");
            try
            {
                _threadRun = true;
                _thread.Start();
                Trace.WriteLine("[Networking] SocketListener thread started.");
            }
            catch(Exception e)
            {
                Trace.WriteLine($"[Networking] Error in starting the thread: {e.Message}.");
            }
		}

        /// <summary>
        /// This function stops the thread.
        /// </summary>
        /// <returns> void </returns>
        public void Stop()
		{
            Trace.WriteLine("[Networking] SocketListener.Stop() function called.");
            _threadRun = false;
			Trace.WriteLine("[Networking] SocketListener thread stopped.");
		}

        /// <summary>
        /// This function is the AsyncCallback function passed to socket.BeginReceive() as an argument.
        /// </summary>
        /// <returns> void </returns>
        private void ReceiveCallback(IAsyncResult ar)
		{
            Trace.WriteLine("[Networking] SocketListener.ReceiveCallback() function called.");
            if (_threadRun)
            {
                try
                {
                    int bytesCount = _socket.EndReceive(ar);
                    if (bytesCount > 0)
                    {
                        _receivedString.Append(Encoding.ASCII.GetString(buffer, 0, bytesCount));
                        string remainingString = ProcessReceivedString(_receivedString.ToString());
                        _receivedString.Clear();
                        _receivedString.Append(remainingString);
                    }
                    _socket.BeginReceive(buffer, 0, bufferSize, 0, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Trace.WriteLine($"[Networking] Error in SocketListener.ReceiveCallback(): {e.Message}");
                }
            }
		}

        /// <summary>
        /// This function processes the packets from the given string, and enqueues the packets.
        /// </summary>
        /// <param name="receivedString"> The string containing packets. </param>
        /// <returns> The remaining string after processing the packets from the given string. </returns>
        private string ProcessReceivedString(string receivedString)
		{
            Trace.WriteLine("[Networking] SocketListener.ProcessReceivedString() function called.");
            while (true)
            {
                int packetBegin = receivedString.IndexOf("BEGIN", StringComparison.Ordinal) + 5;
                int packetEnd = receivedString.IndexOf("END", StringComparison.Ordinal);
                while (packetEnd != -1 && receivedString[(packetEnd - 3)..(packetEnd + 3)] == "NOTEND")
                {
                    packetEnd = receivedString.IndexOf("END", packetEnd + 3, StringComparison.Ordinal);
                }
                if (packetBegin == -1 || packetEnd == -1)
                {
                    break;
                }
                Packet packet = SendString.SendStringToPacket(receivedString[packetBegin..packetEnd]);
                _receivingQueue.Enqueue(packet);
                receivedString = receivedString[(packetEnd + 3)..]; // remove the first packet from the string
                Trace.WriteLine($"[Networking] Received data from module: {packet.moduleOfPacket}.");
            }
            Trace.WriteLine("[Networking] SocketListener.ProcessReceivedString() function exited.");
            return receivedString; // return the remaining string
		}
	}
}
