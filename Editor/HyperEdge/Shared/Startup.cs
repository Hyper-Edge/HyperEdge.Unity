using Grpc.Net.Client;
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
            _grpcChannelProvider?.ShutdownAllChannels();
            _asmManager.Dispose();
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
            MessagePack.Resolvers.StaticCompositeResolver.Instance.Register(
                Cysharp.Serialization.MessagePack.UlidMessagePackResolver.Instance,
                MagicOnion.Resolvers.MagicOnionResolver.Instance,
                HyperEdge.Sdk.Shared.MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Resolvers.GeneratedResolver.Instance,
                MessagePack.Resolvers.StandardResolver.Instance);

            var msgPackoptions = MessagePackSerializerOptions.Standard.WithResolver(MessagePack.Resolvers.StaticCompositeResolver.Instance);
            MessagePackSerializer.DefaultOptions = msgPackoptions;
            //
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new UlidConverter() }
            };
            //
            _grpcChannelProvider = new DefaultGrpcChannelProvider(() => new GrpcChannelOptions()
                {
                    HttpHandler = new Cysharp.Net.Http.YetAnotherHttpHandler()
                    {
                        Http2Only = true
                    },
                    DisposeHttpClient = true,
                }
                //new SslCredentials(File.ReadAllText("Packages/tech.hyperedgelabs.unity-plugin/Settings/cert.pem"))
            );
            GrpcChannelProvider.SetDefaultProvider(_grpcChannelProvider);
            //
            MagicOnion.MagicOnionInitializer.Register();
            //
            // Set MessagePipe
            var builder = new BuiltinContainerBuilder();
            builder.AddMessagePipe();
            var provider = builder.BuildServiceProvider();
            GlobalMessagePipe.SetProvider(provider);
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
