using SmartTasksAPI.Entities;

namespace SmartTasksAPI.Models
{
    /// <summary>
    /// Checklist containing a list of tasks: Schema used in GET requests
    /// </summary>
    public class ChecklistWithTasksDto
    {
        /// <summary>
        /// Unique Id of the checklist
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Name of the checklist
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Color of the checklist in hex format
        /// </summary>
        public string Color { get; set; } = "#ffffff";
        /// <summary>
        /// List of tasks in the checklist
        /// </summary>
        public ICollection<TaskItemDto> Items { get; set; } = new List<TaskItemDto>();
    }
}
