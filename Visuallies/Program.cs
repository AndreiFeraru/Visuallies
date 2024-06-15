using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using QuikGraph;
using System.Diagnostics;

namespace Visuallies;

class Program
{

    private static string? _organization;
    private static string? _project;
    private static string? _personalAccessToken;
    private static IEnumerable<WorkItem>? _workItems;

    static void Main(string[] args)
    {
        if (!TryLoadConfig())
        {
            Console.WriteLine("Invalid config");
            return;
        }

        var epicIds = new[] { 16242, 17548 };

        var workItemsManager = new WorkItemsManager(_organization, _personalAccessToken);

        var workItemRelations = workItemsManager.GetWorkItemRelationsByEpicId(epicIds[0]);

        var workItemIds = workItemRelations
            .SelectMany(link => new[] { link.Source, link.Target })
            .Where(item => item != null)
            .Select(item => item.Id)
            .Distinct();

        _workItems = workItemsManager.GetWorkItemsByIds(workItemIds);

        var relations = workItemRelations
            .Where(r => r.Source != null && r.Target != null)
            .Select(r => (source: r.Source.Id, target: r.Target.Id));

        var graph = new AdjacencyGraph<int, Edge<int>>();
        graph.AddVertexRange(workItemIds);

        var clusteredGraph = new ClusteredAdjacencyGraph<int, Edge<int>>(graph);

        var clusters = new Dictionary<int, ClusteredAdjacencyGraph<int, Edge<int>>>();
        var featureWorkItems = _workItems.Where(i => i.IsFeature());
        foreach (var workItem in featureWorkItems)
        {
            clusters[workItem.Id.Value] = clusteredGraph.AddCluster();
        }

        var userStoryItemIds = _workItems
            .Where(i => i.IsUserStory() && i.Id.HasValue)
            .Select(item => item.Id);
        foreach (var userStoryId in userStoryItemIds)
        {
            var parentId = relations.FirstOrDefault(x => x.target == userStoryId).source;
            if (clusters.TryGetValue(parentId, out var parentCluster))
            {
                parentCluster.AddVertex(userStoryId!.Value);
            }
        }

        clusteredGraph.AddEdgeRange(relations.Where(r => !featureWorkItems.Any(item => item.Id == r.source)).Select(r => new Edge<int>(r.source, r.target)));

        clusteredGraph.AddEdgeRange(clusters.Select(c => new Edge<int>(c.Key, c.Value.Vertices.FirstOrDefault())));

        var visualizer = new GraphVisualizer(_workItems, _organization, _project);
        var output = visualizer.GenerateDotFromGraph(clusteredGraph);

        File.WriteAllText("work_items_graph.dot", output);

        var dotExecutablePath = "C:\\Program Files\\Graphviz\\bin\\dot";
        // Convert DOT to PNG using Graphviz command line tool (requires Graphviz installed) 
        Process.Start(new ProcessStartInfo(dotExecutablePath, "-Tsvg work_items_graph.dot -o graph.svg")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        }).WaitForExit();

        Console.WriteLine("Graph svg generated");
    }
    private static bool TryLoadConfig()
    {
        var configManager = new ConfigManager();

        _organization = configManager.Config[ConfigKeys.ORGANIZATION];
        _project = configManager.Config[ConfigKeys.PROJECT];
        _personalAccessToken = configManager.Config[ConfigKeys.PERSONAL_ACCESS_TOKEN];

        if (_organization == null ||
            _project == null ||
            _personalAccessToken == null)
        {
            Console.WriteLine("Invalid config.");
            return false;
        }

        return true;
    }


    public static WorkItem? GetWorkItemById(int id)
        => _workItems.FirstOrDefault(item => item.Id == id);
}