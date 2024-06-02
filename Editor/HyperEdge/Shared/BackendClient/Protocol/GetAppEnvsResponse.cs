using System;
using System.Collections.Generic;


namespace HyperEdge.Sdk.Unity
{
    public class GetAppEnvsResponse
    {
        public List<AppEnvInfoDTO> Envs { get; set; } = new();
    }

    public class AppEnvInfoDTO
    {
        public string Id { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

