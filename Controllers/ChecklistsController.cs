using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.IdentityModel.Tokens;
using SmartTasksAPI.Entities;
using SmartTasksAPI.Models;
using SmartTasksAPI.Services;
using static System.Net.WebRequestMethods;

namespace SmartTasksAPI.Controllers
{
    [Route("api/v{version:apiVersion}/checklists")]
    [ApiVersion("1.0")]
    [ApiController]
    [Authorize]
    public class ChecklistsController : ControllerBase
    {
        private readonly ISmartTasksRepository _repository;
        private readonly IMapper _mapper;
        private readonly UserManager<UserAccount> _userManager;

        public ChecklistsController(ISmartTasksRepository repository, 
            IMapper mapper, UserManager<UserAccount> userManager)
        {
            _repository = repository;
            _mapper = mapper;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets all checklists for an authenticated user
        /// </summary>
        /// <param name="searchQuery">A query that is searched for in checklist names</param>
        /// <param name="pageNumber">The page number of the page to return</param>
        /// <param name="pageSize">The size of each page</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/checklists
        /// 
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ChecklistDto>>> GetChecklists(
            string? searchQuery, int pageNumber = 1, int pageSize = Constants.DefaultPageSize)
        {
            // Add range restrictions to pageSize and pageNumber
            if (pageSize > Constants.MaxPageSize)
            {
                pageSize = Constants.MaxPageSize;
            }

            
            if (pageSize < 1 || pageNumber < 1)
            {
                return BadRequest("Page size and page number must be greater than 0");
            }

            // Get user
            var user = await _userManager.GetUserAsync(User);

            // Gets checklists and paging data
            var (checklistEntities, paginationMetadata) = await _repository
                .GetChecklistsForUserAsync(user.Id, searchQuery, pageNumber: pageNumber, pageSize: pageSize);

            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));

            return Ok(_mapper.Map<IEnumerable<ChecklistDto>>(checklistEntities));
        }


        /// <summary>
        /// Gets the checklist with ID checklistId including its list of tasks
        /// </summary>
        /// <param name="checklistId">The Id of the checklist to return</param>
        /// <returns></returns>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/checklists/1
        /// 
        /// </remarks>
        [HttpGet("{checklistId}", Name="GetChecklist")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ChecklistWithTasksDto>> GetChecklist(int checklistId)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Gets requested checklist
            var checklistEntity = await _repository.GetChecklistAsync(checklistId, includeTasks: true);
            return Ok(_mapper.Map<ChecklistWithTasksDto>(checklistEntity));
        }

        /// <summary>
        /// Creates a checklist from the passed in parameters
        /// </summary>
        /// <param name="checklist"></param>
        /// <remarks>
        /// Note: Pass in the color in HEX format.<br/>
        /// 
        /// Sample request:
        ///
        ///     POST /api/v1/checklists
        ///     {
        ///         "name": "new list",
        ///         "color": "#FFFFFF"
        ///     }
        /// 
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ChecklistDto>> CreateChecklist(
            ChecklistForCreationDto checklist)
        {
            // Map to a checklist entity
            var checklistEntity = _mapper.Map<Checklist>(checklist);

            // Set the user Id
            var user = await _userManager.GetUserAsync(User);
            checklistEntity.UserId = user.Id;

            // Save to the database
            await _repository.CreateChecklistAsync(checklistEntity);
            await _repository.SaveChangesAsync();

            // Returns a 201 Created response
            var checklistToReturn = _mapper.Map<ChecklistDto>(checklistEntity);

            return CreatedAtRoute(
                "GetChecklist",
                new { checklistId = checklistToReturn.Id },
                checklistToReturn);
        }

        /// <summary>
        /// Deletes the checklist with ID checklistId and all its contained tasks
        /// </summary>
        /// <param name="checklistId">The Id of the checklist to be deleted</param>
        /// <remarks>
        /// Note: This will delete all tasks contained within the checklist <br/>
        /// 
        /// Sample request:
        ///
        ///     DELETE /api/v1/checklists/1
        ///
        /// </remarks>
        [HttpDelete("{checklistId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteChecklist(int checklistId)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Delete checklist and save changes
            await _repository.DeleteChecklistAsync(checklistId);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Updates the checklist with ID checklistId
        /// </summary>
        /// <param name="checklistId">The Id of the checklist to be updated</param>
        /// <param name="checklist"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/v1/checklists/1
        ///     {
        ///         "name": "updated name",
        ///         "color": "#FFFFFF"
        ///     }
        /// 
        /// </remarks>
        [HttpPut("{checklistId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> UpdateChecklist(
            int checklistId, ChecklistForUpdateDto checklist)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Ensure checklist exists
            var checklistEntity = await _repository.GetChecklistAsync(checklistId);

            // Copy the values over to the database and save
            _mapper.Map(checklist, checklistEntity);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Partially updates the checklist with ID checklistId
        /// </summary>
        /// <param name="checklistId">The Id of the checklist to update</param>
        /// <param name="patchDocument"></param>
        /// <remarks>
        /// Note: See <a href="https://jsonpatch.com">Json patch</a> for more information.<br/>
        ///  
        /// Sample request:
        /// 
        ///     PATCH /api/v1/checklists/1
        ///     [{
        ///         "op": "replace",
        ///         "path": "/name",
        ///         "value": "New name" 
        ///     }]
        /// 
        /// </remarks>
        [HttpPatch("{checklistId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> PartiallyUpdateChecklist(
            int checklistId, JsonPatchDocument<ChecklistForUpdateDto> patchDocument)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Ensure checklists exists
            var checklistEntity = await _repository.GetChecklistAsync(checklistId);

            // Create a temporary variable to apply the patch document
            var checklistToPatch = _mapper.Map<ChecklistForUpdateDto>(checklistEntity);
            patchDocument.ApplyTo(checklistToPatch, ModelState);

            // Check that validation attributes are satisfied after the patch
            if (!ModelState.IsValid || !TryValidateModel(checklistToPatch))
            {
                return BadRequest(ModelState);
            }

            // Copy over values and save changes
            _mapper.Map(checklistToPatch, checklistEntity);
            await _repository.SaveChangesAsync();

            return NoContent();
        }
    }
}
