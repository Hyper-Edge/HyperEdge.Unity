using Grpc.Core;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MagicOnion.Unity;
using MessagePack;
using MessagePipe;
using Newtonsoft.Json;
using UnityEditor;

using HyperEdge.Shared.Utils;


namespace HyperEdge.Sdk.Unity
{
    [InitializeOnLoad]
    public class HyperEdgeStartup
    {
        static IGrpcChannelProvider _grpcChannelProvider = null;
        static CancellationTokenSource _cts = new();
        static AssemblyManager _asmManager = AssemblyManager.Instance;

        static void OnEditorQuit()
        {
            CodeEditorServer.Stop();
            _asmManager.Dispose();
            _grpcChannelProvider?.ShutdownAllChannels();
            _cts.Cancel();
        }

        static HyperEdgeStartup()
        {
            //
            if (!File.Exists("ProjectSettings/requirements.txt"))
            {
                File.Copy("Packages/tech.hyperedgelabs.unity-plugin/Settings/requirements.txt", "ProjectSettings/requirements.txt");
            }
            if (!File.Exists("ProjectSettings/PythonSettings.asset"))
            {
                File.Copy("Packages/tech.hyperedgelabs.unity-plugin/Settings/PythonSettings.asset.txt", "ProjectSettings/PythonSettings.asset");
            }
            //
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new UlidConverter() }
            };
            //
            var msgPackResolver = MessagePack.Resolvers.CompositeResolver.Create(
                Cysharp.Serialization.MessagePack.UlidMessagePackResolver.Instance,
                MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Resolvers.StandardResolver.Instance);

            var msgPackoptions = MessagePackSerializerOptions.Standard.WithResolver(msgPackResolver);
            MessagePackSerializer.DefaultOptions = msgPackoptions;
            
            // Set MessagePipe
            var builder = new BuiltinContainerBuilder();
            builder.AddMessagePipe();
            var provider = builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);
            //
            _grpcChannelProvider = new DefaultGrpcChannelProvider(new GrpcCCoreChannelOptions(new []
                {
                    // send keepalive ping every 5 second, default is 2 hours
                    new ChannelOption("grpc.keepalive_time_ms", 5000),
                    // keepalive ping time out after 5 seconds, default is 20 seconds
                    new ChannelOption("grpc.keepalive_timeout_ms", 5 * 1000),
                },
                new SslCredentials(File.ReadAllText("Packages/tech.hyperedgelabs.unity-plugin/Settings/cert.pem"))
            ));
            GrpcChannelProvider.SetDefaultProvider(_grpcChannelProvider);
            //
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            //
            UniPipeServer.Default.Start(_cts.Token).Forget();
            //
            CodeEditorServer.Start();
            //
            EditorApplication.quitting += OnEditorQuit;
        }
    }
}
