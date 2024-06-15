using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;

namespace Visuallies
{
    internal class GraphVisualizer(IEnumerable<WorkItem> workItems, string organization, string project)
    {
        private IEnumerable<WorkItem> _workItems = workItems;
        private string _organization = organization;
        private string _project = project;
        public const string REPLACE_TOKEN = "[REMOVE_QUOTE_HERE]";

        public string GenerateDotFromGraph(ClusteredAdjacencyGraph<int, Edge<int>> graph)
        {
            var graphviz = new GraphvizAlgorithm<int, Edge<int>>(graph);

            FormatGraph(graphviz);

            FormatClusters(graphviz);

            FormatNodes(graphviz);

            string output = graphviz.Generate();

            output = output.Replace($"\"{REPLACE_TOKEN}", string.Empty);
            output = output.Replace($"{REPLACE_TOKEN}\"", string.Empty);

            return output;
        }

        private void FormatNodes(GraphvizAlgorithm<int, Edge<int>> graphviz)
        {
            graphviz.FormatVertex += (sender, e) =>
            {
                var item = _workItems.FirstOrDefault(item => item.Id == e.Vertex);
                var itemType = item.GetWorkItemType();
                var itemState = item.GetWorkItemState();
                var assignedTo = item.GetWorkItemAssignedTo();

                e.VertexFormat.Font = new GraphvizFont("segoe ui", 14);
                e.VertexFormat.FontColor = GraphvizColor.White;
                e.VertexFormat.Shape = GraphvizVertexShape.Plaintext;
                e.VertexFormat.Style = GraphvizVertexStyle.Filled;
                e.VertexFormat.Url = $"https://dev.azure.com/{_organization}/{_project}/_workitems/edit/{item.Id}";
                e.VertexFormat.ToolTip = $"State: {itemState} | Assigned to: {assignedTo?.DisplayName ?? "Unassigned"}";
                e.VertexFormat.FillColor = itemType switch
                {
                    WorkItemTypes.EPIC => Colours.EPIC,
                    WorkItemTypes.FEATURE => Colours.FEATURE,
                    WorkItemTypes.USER_STORY => Colours.USER_STORY
                };

                FormatNodeLabel(e, item, itemState);
            };
        }

        private static void FormatNodeLabel(FormatVertexEventArgs<int> e, WorkItem? item, string itemState)
        {
            string circleColor = itemState switch
            {
                "New" => Colours.GRAY,
                "Active" => Colours.BLUE,
                "Resolved" => Colours.GREEN,
                "Closed" => Colours.GREEN,
                _ => Colours.RED
            };
            string statusCircle = $"<FONT COLOR='{circleColor}' POINT-SIZE='30'>&#x25CF;</FONT>";
            e.VertexFormat.Label = @$"{REPLACE_TOKEN}<
                <TABLE BORDER='0' CELLBORDER='0' CELLPADDING='2' CELLSPACING='0'>
                    <TR>
                        <TD>{statusCircle}</TD>
                        <TD>{item?.Id?.ToString() ?? string.Empty} - {item.GetWorkItemTitle() ?? string.Empty}</TD>
                    </TR>
                </TABLE>
            >{REPLACE_TOKEN}";
        }

        private static void FormatGraph(GraphvizAlgorithm<int, Edge<int>> graphviz)
        {
            graphviz.GraphFormat.RankDirection = GraphvizRankDirection.LR;
            graphviz.GraphFormat.BackgroundColor = GraphvizColor.Transparent;
            graphviz.GraphFormat.Font = new GraphvizFont("segoe ui", 14);
        }

        private static void FormatClusters(GraphvizAlgorithm<int, Edge<int>> graphviz)
        {
            graphviz.FormatCluster += (sender, e) =>
            {
                e.GraphFormat.BackgroundColor = GraphvizColor.LightGray;
            };
        }

    }
}
