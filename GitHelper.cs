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
    [ClientSample(GitWebApiConstants.AreaName, "pushes")]
    public class PushesSample : ClientSample
    {
        [ClientSampleMethod]
        public IEnumerable<GitPush> ListPushesIntoMaster()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();

            TeamProjectReference project = ClientSampleHelpers.FindAnyProject(this.Context);
            GitRepository repo = GitSampleHelpers.FindAnyRepository(this.Context, project.Id);

            List<GitPush> pushes = gitClient.GetPushesAsync(
                repo.Id,
                searchCriteria: new GitPushSearchCriteria()
                {
                    IncludeRefUpdates = true,
                    RefName = "refs/heads/master",
                }).Result;

            Console.WriteLine("project {0}, repo {1}", project.Name, repo.Name);
            foreach (GitPush push in pushes)
            {
                Console.WriteLine("push {0} by {1} on {2}",
                    push.PushId, push.PushedBy.DisplayName, push.Date);
            }

            return pushes;
        }

        [ClientSampleMethod]
        public IEnumerable<GitPush> ListPushesInLast10Days()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();

            TeamProjectReference project = ClientSampleHelpers.FindAnyProject(this.Context);
            GitRepository repo = GitSampleHelpers.FindAnyRepository(this.Context, project.Id);

            List<GitPush> pushes = gitClient.GetPushesAsync(
                repo.Id,
                searchCriteria: new GitPushSearchCriteria()
                {
                    FromDate = DateTime.UtcNow - TimeSpan.FromDays(10),
                    ToDate = DateTime.UtcNow,
                }).Result;

            Console.WriteLine("project {0}, repo {1}", project.Name, repo.Name);
            foreach (GitPush push in pushes)
            {
                Console.WriteLine("push {0} by {1} on {2}",
                    push.PushId, push.PushedBy.DisplayName, push.Date);
            }

            return pushes;
        }

        [ClientSampleMethod]
        public GitPush CreatePush()
        {
            VssConnection connection = this.Context.Connection;
            GitHttpClient gitClient = connection.GetClient<GitHttpClient>();

            TeamProjectReference project = ClientSampleHelpers.FindAnyProject(this.Context);
            GitRepository repo = GitSampleHelpers.FindAnyRepository(this.Context, project.Id);

            // we will create a new push by making a small change to the default branch
            // first, find the default branch
            string defaultBranchName = GitSampleHelpers.WithoutRefsPrefix(repo.DefaultBranch);
            GitRef defaultBranch = gitClient.GetRefsAsync(repo.Id, filter: defaultBranchName).Result.First();

            // next, craft the branch and commit that we'll push
            GitRefUpdate newBranch = new GitRefUpdate()
            {
                Name = $"refs/heads/vsts-api-sample/{GitSampleHelpers.ChooseRefsafeName()}",
                OldObjectId = defaultBranch.ObjectId,
            };
            string newFileName = $"{GitSampleHelpers.ChooseItemsafeName()}.md";
            GitCommitRef newCommit = new GitCommitRef()
            {
                Comment = "Add a sample file",
                Changes = new GitChange[]
                {
                    new GitChange()
                    {
                        ChangeType = VersionControlChangeType.Add,
                        Item = new GitItem() { Path = $"/vsts-api-sample/{newFileName}" },
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

            Console.WriteLine("project {0}, repo {1}", project.Name, repo.Name);
            Console.WriteLine("push {0} updated {1} to {2}",
                push.PushId, push.RefUpdates.First().Name, push.Commits.First().CommitId);

            // now clean up after ourselves (and in case logging is on, don't log these calls)
            ClientSampleHttpLogger.SetSuppressOutput(this.Context, true);

            // delete the branch
            GitRefUpdateResult refDeleteResult = gitClient.UpdateRefsAsync(
                new GitRefUpdate[]
                {
                    new GitRefUpdate()
                    {
                        OldObjectId = push.RefUpdates.First().NewObjectId,
                        NewObjectId = new string('0', 40),
                        Name = push.RefUpdates.First().Name,
                    }
                },
                repositoryId: repo.Id).Result.First();

            // pushes and commits are immutable, so no way to clean them up
            // but the commit will be unreachable after this

            return push;
        }
    }
}