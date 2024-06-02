using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.UnityLinker;


namespace HyperEdge.Sdk.Unity.Flexi
{
    public class LinkerProcessor : IUnityLinkerProcessor
    {
        public int callbackOrder => 0;

        public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
        {
            const string filePath = "Packages/HyperEdge.Sdk.Unity/link.xml";
            return Path.GetFullPath(filePath);
        }

	public void OnBeforeRun(BuildReport report, UnityLinkerBuildPipelineData data)
	{
	}

	public void OnAfterRun(BuildReport report, UnityLinkerBuildPipelineData data)
	{
	}
    }
}
