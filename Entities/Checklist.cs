using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace SmartTasksAPI.Entities
{
    /// <summary>
    /// Checklist containing a list of tasks
    /// </summary>
    public class Checklist
    {
        /// <summary>
        /// Unique Id of the checklist
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Name of the checklist
        /// </summary>
        [Required]
        [StringLength(Constants.MaxChecklistNameLength)]
        public string Name { get; set; } = null!;

        /// <summary>
        /// Color of the checklist in hex format
        /// </summary>
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
            ErrorMessage = "Invalid hex color format")]
        public string Color { get; set; } = "#ffffff";

        /// <summary>
        /// List of tasks in the checklist
        /// </summary>
        public ICollection<TaskItem> Items { get; set; } = new List<TaskItem>();
        
        /// <summary>
        /// Associated user
        /// </summary>
        [Required]
        [ForeignKey("UserId")]
        public UserAccount User { get; set; } = null!;

        public string UserId { get; set; } = null!;
    }
}
