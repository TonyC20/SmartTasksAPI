using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Models
{
    /// <summary>
    /// Checklist: Schema used in PUT and PATCH requests
    /// </summary>
    public class ChecklistForUpdateDto
    {
        /// <summary>
        /// Name of the checklist
        /// </summary>
        [Required]
        [StringLength(Constants.MaxChecklistNameLength)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Color of the checklist in hex format
        /// </summary>
        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$",
            ErrorMessage = "Invalid hex color format")]
        public string Color { get; set; } = "#ffffff";
    }
}
