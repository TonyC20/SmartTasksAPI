namespace SmartTasksAPI.Models
{
    /// <summary>
    /// Contains information for a task. Schema used for GET requests
    /// </summary>
    public class TaskItemDto
    {
        /// <summary>
        /// Unique Id of the task
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the task
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description for the task
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Date and time of the task in ISO 8601 format
        /// </summary>
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Task priority level
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Whether or not the task is completed
        /// </summary>
        public bool IsCompleted { get; set; } = false;
    }
}
