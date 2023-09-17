using System.Runtime.InteropServices;
using System.Security;
using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.DbContexts;
using SmartTasksAPI.Entities;

namespace SmartTasksAPI.Services
{
    public class SmartTasksRepository : ISmartTasksRepository
    {
        private readonly SmartTasksContext _context;
        public SmartTasksRepository(SmartTasksContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Checklist>, PaginationMetadata)> GetChecklistsForUserAsync(string userId,
            string? searchQuery, int pageNumber = 1, int pageSize = Constants.DefaultPageSize)
        {
            var collection = _context.Checklists as IQueryable<Checklist>;

            // Filter by userId
            collection = collection.Where(c => c.UserId.Equals(userId));

            // Filter by search query
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(c => c.Name.Contains(searchQuery));
            }

            // Create pagination metadata
            var totalItemCount = await collection.CountAsync();
            var paginationMetadata = new PaginationMetadata(
                totalItemCount, pageSize, pageNumber);

            // Return results of the query
            var finalList = await collection.Skip(pageSize * (pageNumber - 1))
                .Take(pageSize)
                .OrderBy(c => c.Name)
                .ToListAsync();

            return (finalList, paginationMetadata);

        }

        public async Task<Checklist?> GetChecklistAsync(int checklistId, bool includeTasks = false)
        {
            var collection = _context.Checklists.Where(c => c.Id == checklistId);

            // Whether to include tasks
            if (includeTasks)
            {
                collection = collection.Include(c => c.Items);
            }

            var checklist = await collection.FirstOrDefaultAsync();
            return checklist;
        }

        public async Task CreateChecklistAsync(Checklist checklist)
        {
            _context.Checklists.Add(checklist);
        }

        public async Task DeleteChecklistAsync(int checklistId)
        {
            var checklist = await GetChecklistAsync(checklistId)
                            ?? throw new Exception("Checklist to delete was not found");

            // Delete items in checklist
            foreach (var item in checklist.Items)
            {
                _context.TaskItems.Remove(item);
            }

            // Delete checklist
            _context.Checklists.Remove(checklist);
        }

        public async Task<bool> ChecklistExistsAsync(int checklistId)
        {
            return await _context.Checklists
                .Where(c => c.Id == checklistId).AnyAsync();
        }

        public async Task<bool> ChecklistExistsAndByUserAsync(int checklistId, string userId)
        {
            var result = await _context.Checklists
                .Where(c => c.Id == checklistId && c.UserId == userId)
                .AnyAsync();

            return result;
        }

        public async Task<IEnumerable<TaskItem>> GetTasksAsync(int checklistId)
        {
            var tasks = await _context.TaskItems.Where(t =>
                t.ChecklistId == checklistId).OrderBy(t => t.Name).ToListAsync();

            return tasks;
        }

        public async Task<TaskItem?> GetTaskAsync(int checklistId, int taskId)
        {
            var task = await _context.TaskItems.Where(t =>
                t.Id == taskId && t.ChecklistId == checklistId).FirstOrDefaultAsync();

            return task;
        }

        public async Task AddTaskForChecklistAsync(int checklistId, TaskItem taskItem)
        {
            taskItem.ChecklistId = checklistId;
            _context.TaskItems.Add(taskItem);
        }

        public async Task DeleteTaskAsync(TaskItem taskItem)
        {
            _context.TaskItems.Remove(taskItem);
        }


        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
