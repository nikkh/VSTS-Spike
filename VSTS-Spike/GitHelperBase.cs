using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.WebApi;

namespace VSTS_Spike
{
    public abstract class GitHelperBase
    {
        public VssConnection Connection { get; set; }
    }
}
