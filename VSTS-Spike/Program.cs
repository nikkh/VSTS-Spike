using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Operations;
using System.Threading;

namespace VSTS_Spike
{
    class Program
    {
        const string c_collectionUri = "https://nicks-ms-subscription.visualstudio.com/DefaultCollection";
        const string c_projectname = "xekina";
        const string c_reponame = "xekina";
        static void Main(string[] args)
        {
            VssCredentials creds = new VssClientCredentials();
            creds.Storage = new VssClientCredentialStorage();
            VssConnection connection = new VssConnection(new Uri(c_collectionUri), creds);
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
            var repo = gitClient.GetRepositoryAsync(c_projectname, c_reponame).Result;
            WorkHttpClient w  =connection.GetClient<WorkHttpClient>();
            ProcessHttpClient processClient = connection.GetClient<ProcessHttpClient>();
            var processes = processClient.GetProcessesAsync().Result;
            
            TeamContext tc = new TeamContext(c_projectname);
            var teamIterations = w.GetTeamIterationsAsync(tc).Result;
            var processConfiguration = w.GetProcessConfigurationAsync(c_projectname).Result;
            var boards = w.GetBoardsAsync(tc).Result;
            foreach (var board in boards)
            {
                Console.WriteLine(board.Name);
            }
           

            // and retrieve the corresponding project client 
            var projectHttpClient = connection.GetClient<ProjectHttpClient>();

            // then - same as above.. iterate over the project references (with a hard-coded pagination of the first 10 entries only)
            foreach (var projectReference in projectHttpClient.GetProjects(top: 10, skip: 0).Result)
            {
                // and then get ahold of the actual project
                var teamProject = projectHttpClient.GetProject(projectReference.Id.ToString()).Result;
                var urlForTeamProject = ((ReferenceLink)teamProject.Links.Links["web"]).Href;
               
                Console.WriteLine("Team Project '{0}' (Id: {1}) at Web Url: '{2}' & API Url: '{3}'",
                teamProject.Name,
                teamProject.Id,
                urlForTeamProject,
                teamProject.Url);
                
            }
            // return;
            // We can also create new projects, i.e. like this:
            var newTeamProjectToCreate = new TeamProject();
            var somewhatRandomValueForProjectName = (int)(DateTime.UtcNow - DateTime.UtcNow.Date).TotalSeconds;

            // mandatory information is name,
            newTeamProjectToCreate.Name = $"Dummy Project {somewhatRandomValueForProjectName}";

            // .. description
            newTeamProjectToCreate.Description = $"This is a dummy project";

            // and capabilities need to be provided
            newTeamProjectToCreate.Capabilities = new Dictionary<string, Dictionary<string, string>>
{
{
        // particularly which version control the project shall use (as of writing 'TFVC' and 'Git' are available
        "versioncontrol", new Dictionary<string, string>()
{
{"sourceControlType", "TFVC"}
}
},
{
        // and which Process Template to use
        "processTemplate", new Dictionary<string, string>()
{
{"templateTypeId", "6008e993-7062-40b0-9450-0b699b103615"} // This is the Id for the Agile template, on my TFS server at least.
        }
}
};

            // because project creation takes some time on the server, the creation is queued and you'll get back a 
            // ticket / reference to the operation which you can use to track the progress and/or completion
            var operationReference = projectHttpClient.QueueCreateProject(newTeamProjectToCreate).Result;

            Console.WriteLine("Project '{0}' creation is '{1}'", newTeamProjectToCreate.Name, operationReference.Status);

            // tracking the status via a OperationsHttpClient (for the Project collection
            var operationsHttpClientForKnownProjectCollection = connection.GetClient<OperationsHttpClient>();

            var projectCreationOperation = operationsHttpClientForKnownProjectCollection.GetOperation(operationReference.Id).Result;
            while (projectCreationOperation.Status != OperationStatus.Succeeded
            && projectCreationOperation.Status != OperationStatus.Failed
            && projectCreationOperation.Status != OperationStatus.Cancelled)
            {
                Console.Write(".");
                Thread.Sleep(1000); // yuck

                projectCreationOperation = operationsHttpClientForKnownProjectCollection.GetOperation(operationReference.Id).Result;
            }

            // alright - creation is finished, successfully or not
            Console.WriteLine("Project '{0}' creation finished with State '{1}' & Message: '{2}'",
            newTeamProjectToCreate.Name,
            projectCreationOperation.Status,
            projectCreationOperation.ResultMessage ?? "n.a.");

            Console.ReadLine();
            return;
        }
    }
}
