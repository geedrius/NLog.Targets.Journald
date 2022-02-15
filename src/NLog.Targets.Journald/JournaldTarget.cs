using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Mono.Unix;
using NLog.Config;

namespace NLog.Targets
{

    /// <summary>
    /// References:
    /// https://systemd.io/JOURNAL_NATIVE_PROTOCOL/
    /// https://stackoverflow.com/a/40203940
    /// </summary>
    [Target("Journald")]
    public class JournaldTarget: TargetWithLayout
    {
        /// <summary>
        /// Optional. Emitted as SYSLOG_IDENTIFIER journal field.
        /// </summary>
        public string SysLogIdentifier { get; set; }
    
        private const string SystemdJournalSocket = "/run/systemd/journal/socket";

        // encoding used to write journald output
        private static readonly Encoding Enc = Encoding.UTF8;
        
        // expect long stack traces to be logged
        private readonly byte[] _buffer = new byte[32*1024];

        // unix domain socket used to write journald output
        private Socket _socket;
        
        /// <summary>
        /// Writes logging event to the log target.
        /// classes.
        /// </summary>
        /// <param name="logEvent">
        /// Logging event to be written out.
        /// </param>
        protected override void Write(LogEventInfo logEvent)
        {
            try
            {
                var socket = GetJournaldSocket();
                var datagramSize = WriteDatagram(logEvent, _buffer, 0);
                socket.Send(_buffer, 0, datagramSize, SocketFlags.None);
            }
            catch (Exception ex)
            {
                Common.InternalLogger.Warn(ex, "Journald write failed");
                throw;
            }
        }
        
        protected override void Dispose(bool disposing)
        {
            Cleanup();
            base.Dispose(disposing);
        }

        protected override void CloseTarget()
        {
            Cleanup();
            base.CloseTarget();
        }
        
        /// <summary>
        /// Writes Journald native protocol datagram to given buffer at given position
        /// </summary>
        private int WriteDatagram(LogEventInfo logEvent, byte[] buffer, int position)
        {
            var count = 0;
            // standard journald fields
            count += WriteField("PRIORITY", GetJournaldPriority(logEvent.Level), buffer, position);
            count += WriteField("MESSAGE", Layout.Render(logEvent), buffer, position + count);
            // custom fields
            count += WriteField("LEVEL", logEvent.Level.ToString(), buffer, position + count);
            count += WriteField("LOGGER", logEvent.LoggerName, buffer, position + count);
            count += WriteField("TIMESTAMP", logEvent.TimeStamp.ToString("O"), buffer, position + count);
            if (!string.IsNullOrEmpty(SysLogIdentifier))
            {
                count += WriteField("SYSLOG_IDENTIFIER", SysLogIdentifier, buffer, position + count);
            }
            if (logEvent.Exception != null)
            {
                count += WriteField("EXCEPTION_TYPE", logEvent.Exception.GetType().FullName, buffer, position + count);
                count += WriteField("EXCEPTION_MESSAGE", logEvent.Exception.Message, buffer, position + count);
                if (logEvent.Exception.StackTrace != null)
                {
                    count += WriteField("EXCEPTION_STACKTRACE", logEvent.Exception.StackTrace, buffer, position + count);
                }
            }
            if (logEvent.HasStackTrace)
            {
                count += WriteField("STACKTRACE", logEvent.StackTrace.ToString(), buffer, position + count);
            }
            return count;
        }

        /// <summary>
        /// Maps NLog level to Journald priority
        /// </summary>
        private string GetJournaldPriority(LogLevel level)
        {
            string priority;
            if (level == LogLevel.Fatal)
            {
                priority = "2"; // Critical
            }
            else if (level == LogLevel.Error)
            {
                priority = "3"; // Error  
            }
            else if (level == LogLevel.Warn)
            {
                priority = "4"; // Warning
            }
            else if (level == LogLevel.Info)
            {
                priority = "6"; // Info
            }
            else // Debug, Trace
            {
                priority = "7"; // Debug
            }
            return priority;
        }
        
        /// <summary>
        /// Writes single datagram field (key and value) to given buffer at given position
        /// </summary>
        private int WriteField(string key, string value, byte[] buffer, int position)
        {
            // write field containing new lines
            if (value.Contains("\n"))
            {
                var count = 0;
                
                // write key
                count += Enc.GetBytes(key, 0, key.Length, buffer, position);
                buffer[position + count] = (byte) '\n'; count++;

                // write value
                const int lengthSize = 8;
                var lengthPos = position + count;  // store buffer position for field value length bytes  
                count += lengthSize;       // reserve 8 bytes in buffer for field value length bytes 
                var valueLength = Enc.GetBytes(value, 0, value.Length, buffer, position + count);
                count += valueLength;
                var lengthBytes = BitConverter.GetBytes((long)valueLength);
                if (!BitConverter.IsLittleEndian)
                {
                    lengthBytes = lengthBytes.Reverse().ToArray();
                }
                if (lengthBytes.Length != lengthSize)
                    throw new Exception($"Failed to convert value for {key}");
                Buffer.BlockCopy(lengthBytes, 0, buffer, lengthPos, lengthSize);

                buffer[position + count] = (byte) '\n'; count++;
                return count;
            }
            // write simple field
            else
            {
                var count = 0;
                count += Enc.GetBytes(key, 0, key.Length, buffer, position);
                buffer[position+count] = (byte) '='; count++;
                count += Enc.GetBytes(value, 0, value.Length, buffer, position+count);
                buffer[position+count] = (byte) '\n'; count++;
                return count;
            }
        }
        
        private Socket GetJournaldSocket()
        {
            if (_socket == null)
            {
                _socket = GetConnectedSocket(SystemdJournalSocket);
            }
            else if (!_socket.Connected)
            {
                Cleanup();
                _socket = GetConnectedSocket(SystemdJournalSocket);
            }

            return _socket;
        }

        private Socket GetConnectedSocket(string socketPath)
        {
            var socket = new Socket(AddressFamily.Unix, SocketType.Dgram, ProtocolType.Unspecified)
            {
                SendBufferSize = _buffer.Length
            };
            Common.InternalLogger.Info("Journald socket connect");
            socket.Connect(new UnixEndPoint(socketPath));
            return socket;
        }

        private void Cleanup()
        {
            try
            {
                Common.InternalLogger.Info("Journald socket cleanup");
                _socket?.Dispose();
            }
            catch (Exception ex)
            {
                Common.InternalLogger.Warn(ex, "Journald socket cleanup failed");
            }
            finally
            {
                _socket = null;
            }
        }
    }
}