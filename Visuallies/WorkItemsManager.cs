using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;

namespace Visuallies;

public class WorkItemsManager(string organization, string personalAccessToken)
{
    private readonly WorkItemTrackingHttpClient _client = InstantiateClient(organization, personalAccessToken);

    public IEnumerable<WorkItemLink> GetWorkItemRelationsByEpicId(int epicId)
    {
        var wiql = new Wiql()
        {
            Query = $@"
                SELECT [System.Id]
                FROM WorkItemLinks 
                WHERE [Source].[System.Id] = {epicId} 
                AND ([System.Links.LinkType] = 'System.LinkTypes.Hierarchy-Forward') 
                AND ([Target].[System.WorkItemType] IN ('Epic', 'Feature', 'User Story'))
                AND ([Source].[System.State] != 'Removed')
                AND ([Target].[System.State] != 'Removed')
                MODE (Recursive)"
        };
        var queryResult = _client.QueryByWiqlAsync(wiql).Result;
        return queryResult.WorkItemRelations;
    }

    public IEnumerable<WorkItem> GetWorkItemsByIds(IEnumerable<int> workItemIds)
    {
        var fields = new[] { "System.Id", "System.State", "System.Title", "System.WorkItemType", "System.AssignedTo" };
        return _client.GetWorkItemsAsync(workItemIds, fields).Result;
    }

    private static WorkItemTrackingHttpClient InstantiateClient(string organization, string personalAccessToken)
    {
        var credentials = new VssBasicCredential(string.Empty, personalAccessToken);
        var baseUri = new Uri($"https://dev.azure.com/{organization}");
        return new WorkItemTrackingHttpClient(baseUri, credentials);
    }
}