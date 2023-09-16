using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Models
{
    /// <summary>
    /// Checklist: Schema used in GET requests
    /// </summary>
    public class ChecklistDto
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
    }
}
