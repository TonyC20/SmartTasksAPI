using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Versioning;


namespace SmartTasksAPI.Entities
{
    /// <summary>
    /// User account for the app.
    /// Can add custom properties in the future
    /// </summary>
    public class UserAccount: IdentityUser
    {
    }
}
