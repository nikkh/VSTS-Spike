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
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using DeploymentHelper;
using Microsoft.Azure;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using VSTS_Spike.Models;

namespace VSTS_Spike
{
    class Program
    {
        const string c_collectionUri = "https://nicks-ms-subscription.visualstudio.com/DefaultCollection";
        const string c_projectname = "xekina";
        const string c_reponame = "xekina";
        static VssConnection v = null;
        static void Main(string[] args)
        {
            VssCredentials creds = new VssClientCredentials();
            creds.Storage = new VssClientCredentialStorage();
            VssConnection connection = new VssConnection(new Uri(c_collectionUri), creds);
            v = connection;

            // Dont need this code at the moment.
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
            var repo = gitClient.GetRepositoryAsync(c_projectname, c_reponame).Result;
            
            //// Get process templates
            //ProcessHttpClient processClient = connection.GetClient<ProcessHttpClient>();
            //var processes = processClient.GetProcessesAsync().Result;




            // Retrieve a list of projects from the account. (and ask if they should be deleted).  This is to tidy up my test data.
            Console.WriteLine("Preparing a list of projects you may wish to delete!");
            var projectHttpClient = connection.GetClient<ProjectHttpClient>();

            //then - same as above..iterate over the project references(with a hard-coded pagination of the first 10 entries only)
            foreach (var projectReference in projectHttpClient.GetProjects(top: 20, skip: 0).Result)
            {
                var teamProject = projectHttpClient.GetProject(projectReference.Id.ToString()).Result;
                //var urlForTeamProject = ((ReferenceLink)teamProject.Links.Links["web"]).Href;

                //Console.WriteLine("Team Project '{0}' (Id: {1}) at Web Url: '{2}' & API Url: '{3}'",
                //teamProject.Name,
                //teamProject.Id,
                //urlForTeamProject,
                //teamProject.Url);
                if (teamProject.Description == "This is a dummy project")
                {
                    Console.WriteLine("Delete this project {0}? (Y/N) default=No", teamProject.Name);
                    string s = Console.ReadLine();
                    if (s == "Y") projectHttpClient.QueueDeleteProject(teamProject.Id);
                }
            }
            // return;

            bool createVSTSProject = false;
            string p = null;
            if (createVSTSProject)
            {
                // Create a new project
                Console.WriteLine("***************Project Creation ******************");
                Console.WriteLine("Enter your project name");
                p = null;
                do
                {
                    p = Console.ReadLine();
                } while (String.IsNullOrEmpty(p));


                projectHttpClient = connection.GetClient<ProjectHttpClient>();
                string projectName = CreateProject(connection, projectHttpClient, p);
                var createdProject = projectHttpClient.GetProject(projectName).Result;

                Console.WriteLine("retrieve and delete default iterations from project");
                WorkHttpClient workClient = connection.GetClient<WorkHttpClient>();
                TeamContext tc = new TeamContext(createdProject.Name);
                var iterations = workClient.GetTeamIterationsAsync(tc).Result;
                foreach (var item in iterations)
                {

                    workClient.DeleteTeamIterationAsync(tc, item.Id).SyncResult();
                    Console.WriteLine("Deleting {0}", item.Name);
                }

                DateTime projectStartDate = new DateTime(2017, 07, 01);
                int discoveryIterationDays = 30;
                int breathingSpaceDays = 3;
                int standardIterationDays = 14;
                int alphaStandardIterations = 4;
                int privateBetaStandardIterations = 4;
                int publicBetaStandardIterations = 6;

                DateTime startDate = projectStartDate;
                DateTime endDate = startDate.AddDays(discoveryIterationDays);

                // Create a discovery iteration to get hold of a node to use as the re-assignment node when deleting stock iterations.
                Console.WriteLine("Creating a Discovery Phase from {0} to {1}", startDate, endDate);
                var node = AddIteration(createdProject.Id, "Discovery", startDate, endDate);
                int discoveryNodeId = node.Id;
                TeamSettingsIteration tsi = new TeamSettingsIteration();
                tsi.Id = node.Identifier;
                var x = workClient.PostTeamIterationAsync(tsi, tc).Result;

                // Delete the standard iterations that come with a new project
                WorkItemTrackingHttpClient workItemTrackingClient = v.GetClient<WorkItemTrackingHttpClient>();
                workItemTrackingClient.DeleteClassificationNodeAsync(createdProject.Id, TreeStructureGroup.Iterations, "Iteration 1", discoveryNodeId).SyncResult();
                workItemTrackingClient.DeleteClassificationNodeAsync(createdProject.Id, TreeStructureGroup.Iterations, "Iteration 2", discoveryNodeId).SyncResult();
                workItemTrackingClient.DeleteClassificationNodeAsync(createdProject.Id, TreeStructureGroup.Iterations, "Iteration 3", discoveryNodeId).SyncResult();

                // Dont really need this at the moment
                // var processConfiguration = workClient.GetProcessConfigurationAsync(projectName).Result;

                // Add Alpha Phase
                startDate = endDate.AddDays(breathingSpaceDays);
                endDate = startDate.AddDays((alphaStandardIterations + 2 * standardIterationDays));
                Console.WriteLine("Creating an Alpha Phase from {0} to {1}", startDate, endDate);
                node = AddIteration(createdProject.Id, "Alpha", startDate, endDate);
                tsi = new TeamSettingsIteration();
                tsi.Id = node.Identifier;
                x = workClient.PostTeamIterationAsync(tsi, tc).Result;

                endDate = startDate.AddDays(standardIterationDays);
                Console.WriteLine("Creating an inception iteration from {0} to {1}", startDate, endDate);
                AddIteration(createdProject.Id, "Inception", startDate, endDate, "Alpha");
                tsi = new TeamSettingsIteration();
                tsi.Id = node.Identifier;
                x = workClient.PostTeamIterationAsync(tsi, tc).Result;

                for (int i = 0; i < alphaStandardIterations; i++)
                {
                    startDate = endDate.AddDays(1);
                    endDate = startDate.AddDays(standardIterationDays);
                    Console.WriteLine("Creating a Standard Iteration {0} from {1} to {2}", i + 1, startDate, endDate);
                    AddIteration(createdProject.Id, String.Format("Iteration {0}", i + 1), startDate, endDate, "Alpha");
                    tsi = new TeamSettingsIteration();
                    tsi.Id = node.Identifier;
                    x = workClient.PostTeamIterationAsync(tsi, tc).Result;

                }

                startDate = endDate.AddDays(1);
                endDate = startDate.AddDays(standardIterationDays);
                Console.WriteLine("Creating an conclusion iteration from {0} to {1}", startDate, endDate);
                AddIteration(createdProject.Id, "Conclusion", startDate, endDate, "Alpha");
                tsi = new TeamSettingsIteration();
                tsi.Id = node.Identifier;
                x = workClient.PostTeamIterationAsync(tsi, tc).Result;

                startDate = endDate.AddDays(breathingSpaceDays);
                endDate = startDate.AddDays((privateBetaStandardIterations + publicBetaStandardIterations) * standardIterationDays);
                Console.WriteLine("Creating a Beta Phase from {0} to {1}", startDate, endDate);
                AddIteration(createdProject.Id, "Beta", startDate, endDate);
                tsi = new TeamSettingsIteration();
                tsi.Id = node.Identifier;
                x = workClient.PostTeamIterationAsync(tsi, tc).Result;

                // test putting the sample project into GiT.
                // This would actually retrieve the files from the core template project.


                gitClient = connection.GetClient<GitHttpClient>();
                repo = gitClient.GetRepositoryAsync(createdProject.Name, createdProject.Name).Result;
                GitHelper gh = new GitHelper(connection);
                var pushes = gh.ListPushesIntoMaster(c_projectname, c_reponame);
                var push = gh.CreatePush(createdProject.Name, repo.Name);
                pushes = gh.ListPushesIntoMaster(createdProject.Name, repo.Name);
            }
            // Create a new project
            Console.WriteLine("***************Create a DevTest Lab ******************");
            Console.WriteLine("Enter a deployment name (need to work out how to capture template paramters here");
            Console.WriteLine("or q to quit");
            p = null;
            do
            {
                p = Console.ReadLine();
            } while (String.IsNullOrEmpty(p));
            if (p == "q") return;
            DeployerParameters parameters = new DeployerParameters();
            
            parameters.ResourceGroupName = "Xekina-RG";
            parameters.DeploymentName = "Xekina-Lab-Deployment";
            parameters.ResourceGroupLocation = "North Europe"; // must be specified for creating a new resource group
            parameters.PathToTemplateFile = "./Files/azuredeploy.json";
            parameters.PathToParameterFile = "./Files/azuredeploy.parameters.json";
            // TODO: Get this from app.settings
            parameters.TenantId = CloudConfigurationManager.GetSetting("TenantId");
            parameters.ClientId = CloudConfigurationManager.GetSetting("ClientId");
            parameters.ClientSecret = CloudConfigurationManager.GetSetting("ClientSecret");
            parameters.SubscriptionId = CloudConfigurationManager.GetSetting("SubscriptionId");

            string templateParameters = File.ReadAllText(parameters.PathToParameterFile);
            RootObject ltp = JsonConvert.DeserializeObject<RootObject>(templateParameters);
            ltp.parameters.newLabName.value = "XekinaLab";
            parameters.ParameterFileContent = JsonConvert.SerializeObject(ltp);
            Deployer deployer = new Deployer(parameters);
            deployer.Deploy().SyncResult();
            Console.ReadLine();
            return;
        }

        private static WorkItemClassificationNode AddIteration(Guid projectId, string iterationName, DateTime startDate, DateTime endDate, string path = null)
        {
            WorkItemClassificationNode iteration = new WorkItemClassificationNode
            {
                Name = iterationName,
                StructureType = TreeNodeStructureType.Iteration
            };

            iteration.Attributes = new Dictionary<string, object>
        {
            {"startDate", startDate},
            {"finishDate", endDate}
        };

            //add the iteration to the project

            WorkItemTrackingHttpClient workItemTrackingClient = v.GetClient<WorkItemTrackingHttpClient>();
            var x = workItemTrackingClient.CreateOrUpdateClassificationNodeAsync(iteration, projectId, TreeStructureGroup.Iterations, path).Result;
            return x;
        }

        private static string CreateProject(VssConnection connection, ProjectHttpClient client, string projectName)
        {
            
            // We can also create new projects, i.e. like this:
            var newTeamProjectToCreate = new TeamProject();
            //var somewhatRandomValueForProjectName = projectName;

            // mandatory information is name,
            //newTeamProjectToCreate.Name = $"Project {somewhatRandomValueForProjectName}";
            newTeamProjectToCreate.Name = projectName;
            // .. description
            newTeamProjectToCreate.Description = $"This is a dummy project";

            // and capabilities need to be provided
            newTeamProjectToCreate.Capabilities = new Dictionary<string, Dictionary<string, string>>
{
{
        // particularly which version control the project shall use (as of writing 'TFVC' and 'Git' are available
        "versioncontrol", new Dictionary<string, string>()
{
{"sourceControlType", "Git"}
}
},
{
        // and which Process Template to use
        "processTemplate", new Dictionary<string, string>()
{
{"templateTypeId", "6008e993-7062-40b0-9450-0b699b103615"} // This is the Id for the GDS template, on my TFS server at least.
        }
}
};

            // because project creation takes some time on the server, the creation is queued and you'll get back a 
            // ticket / reference to the operation which you can use to track the progress and/or completion
            var operationReference = client.QueueCreateProject(newTeamProjectToCreate).Result;

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
            Console.WriteLine();
            Console.WriteLine("Project '{0}' creation finished with State '{1}' & Message: '{2}'",
            newTeamProjectToCreate.Name,
            projectCreationOperation.Status,
            projectCreationOperation.ResultMessage ?? "n.a.");
            return newTeamProjectToCreate.Name;
        }
    }
}
