using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS_Spike.Models
{
    public class NewLabName
    {
        public string value { get; set; }
    }

    public class LabVmShutDownTime
    {
        public string value { get; set; }
    }

    public class MaxAllowedVmsPerUser
    {
        public int value { get; set; }
    }

    public class MaxAllowedVmsPerLab
    {
        public int value { get; set; }
    }

    public class AllowedVmSizes
    {
        public string value { get; set; }
    }

    public class ArtifactRepoFolder
    {
        public string value { get; set; }
    }

    public class ArtifactRepoBranch
    {
        public string value { get; set; }
    }

    public class ArtifactRepoDisplayName
    {
        public string value { get; set; }
    }

    public class PilotVMName
    {
        public string value { get; set; }
    }

    public class VMSize
    {
        public string value { get; set; }
    }

    public class VMStorageType
    {
        public string value { get; set; }
    }

    public class Username
    {
        public string value { get; set; }
    }

    public class Password
    {
        public string value { get; set; }
    }

    public class DevBoxTemplateName
    {
        public string value { get; set; }
    }

    public class DevBoxTemplateDescription
    {
        public string value { get; set; }
    }

    public class TestBoxTemplateName
    {
        public string value { get; set; }
    }

    public class TestBoxTemplateDescription
    {
        public string value { get; set; }
    }

    public class GoldenImageTemplateName
    {
        public string value { get; set; }
    }

    public class GoldenImageTemplateDescription
    {
        public string value { get; set; }
    }

    public class DevBoxVMName
    {
        public string value { get; set; }
    }

    public class TestBoxVMName
    {
        public string value { get; set; }
    }

    public class GoldenImageVMName
    {
        public string value { get; set; }
    }

    public class Parameters
    {
        public NewLabName newLabName { get; set; }
        public LabVmShutDownTime labVmShutDownTime { get; set; }
        public MaxAllowedVmsPerUser maxAllowedVmsPerUser { get; set; }
        public MaxAllowedVmsPerLab maxAllowedVmsPerLab { get; set; }
        public AllowedVmSizes allowedVmSizes { get; set; }
        public ArtifactRepoFolder artifactRepoFolder { get; set; }
        public ArtifactRepoBranch artifactRepoBranch { get; set; }
        public ArtifactRepoDisplayName artifactRepoDisplayName { get; set; }
        public PilotVMName pilotVMName { get; set; }
        public VMSize VMSize { get; set; }
        public VMStorageType VMStorageType { get; set; }
        public Username username { get; set; }
        public Password password { get; set; }
        public DevBoxTemplateName devBoxTemplateName { get; set; }
        public DevBoxTemplateDescription devBoxTemplateDescription { get; set; }
        public TestBoxTemplateName testBoxTemplateName { get; set; }
        public TestBoxTemplateDescription testBoxTemplateDescription { get; set; }
        public GoldenImageTemplateName goldenImageTemplateName { get; set; }
        public GoldenImageTemplateDescription goldenImageTemplateDescription { get; set; }
        public DevBoxVMName devBoxVMName { get; set; }
        public TestBoxVMName testBoxVMName { get; set; }
        public GoldenImageVMName goldenImageVMName { get; set; }
    }

    public class RootObject
    {
        public string schema { get; set; }
        public string contentVersion { get; set; }
        public Parameters parameters { get; set; }
    }
}

