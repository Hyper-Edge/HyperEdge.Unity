using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using MessagePipe;
using UnityEngine;


namespace HyperEdge.Sdk.Unity
{
    public static class StreamUtils
    {
        public static async UniTask<byte[]> ReadExactlyAsync(this System.IO.Stream stream, int count, CancellationToken ct)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = await stream.ReadAsync(buffer, offset, count - offset, ct);
                if (read == 0)
                {
                    await UniTask.Delay(1000);
                }
                offset += read;
            }
            System.Diagnostics.Debug.Assert(offset == count);
            return buffer;
        }

        public static async UniTask<uint> ReadUInt32Async(this System.IO.Stream stream, CancellationToken ct)
        {
            var bs = await ReadExactlyAsync(stream, 4, ct);
            return BitConverter.ToUInt32(bs);
        }
    }

    public class UniPipeServer
    {
        private static UniPipeServer _default_instance = new UniPipeServer();
        public static UniPipeServer Default { get => _default_instance; }
        public static string DefaultAddress { get => _default_instance._address; }

        private readonly TcpListener _listener;
        private string _address = string.Empty;

        public UniPipeServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
        }

        public async UniTaskVoid Start(CancellationToken ct)
        {
            try
            {
                _listener.Start();
                _address = _listener.LocalEndpoint.ToString();
                Debug.Log($"UniPipe listening at {_address}");
                //
                while (!ct.IsCancellationRequested)
                {
                    using (var client = await _listener.AcceptTcpClientAsync())
                    using (var stream = client.GetStream())
                    {
                        var msgSize = await stream.ReadUInt32Async(ct);
                        var msgBytes = await stream.ReadExactlyAsync((int)msgSize, ct);
                        var msg = Encoding.UTF8.GetString(msgBytes);
                        MessageHub.Instance.PublishUniPipeMessage(msg);
                    }
                }
            }
            finally
            {
                _listener.Stop();
            }
        }
    }
}

