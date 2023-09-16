using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Models
{
    /// <summary>
    /// Contains information for a task. Schema used for POST requests
    /// </summary>
    public class TaskItemForCreationDto
    {
        /// <summary>
        /// Name of the task
        /// </summary>
        [Required]
        [StringLength(Constants.MaxChecklistNameLength)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description for the task
        /// </summary>
        [StringLength(Constants.MaxTaskItemDescLength)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Date and time of the task in ISO 8601 format
        /// </summary>
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Task priority level
        /// </summary>
        [Range(0, 3)]
        public int Priority { get; set; }

        /// <summary>
        /// Whether or not the task is completed
        /// </summary>
        public bool IsCompleted { get; set; } = false;
    }
}
