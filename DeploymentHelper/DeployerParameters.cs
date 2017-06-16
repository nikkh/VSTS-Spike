using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeploymentHelper
{
    public class DeployerParameters
    {
        public string SubscriptionId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string ResourceGroupName { get; set; }
        public string DeploymentName  { get; set; }
        public string ResourceGroupLocation { get; set; }
        public string PathToTemplateFile  { get; set; }
        public string PathToParameterFile { get; set; }
        public string TenantId { get; set; }
}
}
