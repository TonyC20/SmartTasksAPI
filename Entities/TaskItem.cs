using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartTasksAPI.Entities
{
    /// <summary>
    /// Contains information for a task
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Unique Id of the task
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Name of the task
        /// </summary>
        [Required]
        [StringLength(Constants.MaxChecklistNameLength)]
        public string Name { get; set; } = null!;

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

        /// <summary>
        /// Checklist that the task belongs to
        /// </summary>
        [ForeignKey("ChecklistId")]
        public Checklist? Checklist { get; set; }
        public int ChecklistId { get; set; }
    }
}
