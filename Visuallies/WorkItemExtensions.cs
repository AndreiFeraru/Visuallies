using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace Visuallies
{
    internal static class WorkItemExtensions
    {
        public static string GetWorkItemType(this WorkItem? item)
        {
            return item.TryGetFieldValue("System.WorkItemType", out string? value) ?
                value ?? string.Empty :
                string.Empty;
        }

        public static bool IsEpic(this WorkItem? item)
            => item.GetWorkItemType() == WorkItemTypes.EPIC;

        public static bool IsFeature(this WorkItem? item)
            => item.GetWorkItemType() == WorkItemTypes.FEATURE;

        public static bool IsUserStory(this WorkItem? item)
            => item.GetWorkItemType() == WorkItemTypes.USER_STORY;

        public static string GetWorkItemState(this WorkItem? item)
        {
            return item.TryGetFieldValue("System.State", out string? value) ?
                value ?? string.Empty :
                string.Empty;
        }

        public static IdentityRef? GetWorkItemAssignedTo(this WorkItem? item)
        {
            return item.TryGetFieldValue("System.AssignedTo", out IdentityRef? value) ?
                value ?? null :
                null;
        }

        public static string GetWorkItemTitle(this WorkItem? item)
        {
            return item.TryGetFieldValue("System.Title", out string? value) ?
                value ?? string.Empty :
                string.Empty;
        }

        private static bool TryGetFieldValue<T>(this WorkItem? item, string field, out T? value)
        {
            if (item is null ||
                item.Fields is null ||
                !item.Fields.TryGetValue(field, out T fieldVal))
            {
                value = default;
                return false;
            }
            value = fieldVal;
            return true;
        }
    }
}
