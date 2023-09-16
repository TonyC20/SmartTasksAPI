using SmartTasksAPI.Entities;

namespace SmartTasksAPI.Services;

public interface ISmartTasksRepository
{
    Task<(IEnumerable<Checklist>, PaginationMetadata)> GetChecklistsForUserAsync(string userId, string? searchQuery,
        int pageNumber = 1, int pageSize = Constants.DefaultPageSize);
    Task<Checklist?> GetChecklistAsync(int checklistId);
    Task CreateChecklistAsync(Checklist checklist);
    Task DeleteChecklistAsync(int checklistId);
    Task<bool> ChecklistExistsAsync(int checklistId);
    Task<bool> ChecklistExistsAndByUserAsync(int checklistId, string userId);
    Task<IEnumerable<TaskItem>> GetTasksAsync(int checklistId);
    Task<TaskItem?> GetTaskAsync(int checklistId, int taskId);
    Task AddTaskForChecklistAsync(int checklistId, TaskItem taskItem);
    Task DeleteTaskAsync(TaskItem taskItem);
    Task<int> SaveChangesAsync();
}