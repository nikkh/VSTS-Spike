using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS_Spike
{
    
    public class GitHelper : GitHelperBase
    {
        private GitHelper() { }
        public GitHelper(VssConnection connection)
        {
            this.Connection = connection;
        }

       
       
        public IEnumerable<GitPush> ListPushesIntoMaster(string projectName, string repoName)
        {
            VssConnection connection = this.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
                
            var repo = gitClient.GetRepositoryAsync(projectName, repoName).Result;

            List<GitPush> pushes = gitClient.GetPushesAsync(
                repo.Id,
                searchCriteria: new GitPushSearchCriteria()
                {
                    IncludeRefUpdates = true,
                    RefName = "refs/heads/master",
                }).Result;

            Console.WriteLine("project {0}, repo {1}", projectName, repo.Name);
            foreach (GitPush push in pushes)
            {
                Console.WriteLine("push {0} by {1} on {2}",
                    push.PushId, push.PushedBy.DisplayName, push.Date);
            }

            return pushes;
        }

       
      
        
        public GitPush CreatePush(string projectName, string repoName)
        {
            VssConnection connection = this.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();
            
            var repo = gitClient.GetRepositoryAsync(projectName, repoName).Result;

            //// we will create a new push by making a small change to the default branch
            //// first, find the default branch
            
            //GitRef defaultBranch = gitClient.GetRefsAsync(repo.Id).Result.First();

            // next, craft the branch and commit that we'll push
            GitRefUpdate newBranch = new GitRefUpdate()
            {
                Name = $"refs/heads/master",
                OldObjectId = "0000000000000000000000000000000000000000"
            };
            string newFileName = $"test.md";
            GitCommitRef newCommit = new GitCommitRef()
            {
                Comment = "Add a sample file",
                Changes = new GitChange[]
                {
                    new GitChange()
                    {
                        ChangeType = VersionControlChangeType.Add,
                        Item = new GitItem() { Path = $"/master/{newFileName}" },
                        NewContent = new ItemContent()
                        {
                            Content = "# Thank you for using VSTS!",
                            ContentType = ItemContentType.RawText,
                        },
                    }
                },
            };

            // create the push with the new branch and commit
            GitPush push = gitClient.CreatePushAsync(new GitPush()
            {
                RefUpdates = new GitRefUpdate[] { newBranch },
                Commits = new GitCommitRef[] { newCommit },
            }, repo.Id).Result;

            Console.WriteLine("project {0}, repo {1}", projectName, repo.Name);
            Console.WriteLine("push {0} updated {1} to {2}",
                push.PushId, push.RefUpdates.First().Name, push.Commits.First().CommitId);

            

           

            return push;
        }
    }
}