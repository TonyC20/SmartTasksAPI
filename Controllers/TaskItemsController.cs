using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SmartTasksAPI.Entities;
using SmartTasksAPI.Models;
using SmartTasksAPI.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace SmartTasksAPI.Controllers
{
    [Route("api/v{version:apiVersion}/checklists/{checklistId}/tasks")]
    [ApiVersion("1.0")]
    [Authorize]
    [ApiController]
    public class TaskItemsController : ControllerBase
    {
        private readonly ISmartTasksRepository _repository;
        private readonly IMapper _mapper;
        private readonly UserManager<UserAccount> _userManager;

        public TaskItemsController(ISmartTasksRepository repository, 
            IMapper mapper, UserManager<UserAccount> userManager)
        {
            _repository = repository;
            _mapper = mapper;
            _userManager = userManager;
        }

        /// <summary>
        /// Gets all tasks from the checklist with ID checklistId
        /// </summary>
        /// <param name="checklistId">The Id of the checklist</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET api/v1/checklists/1/tasks
        ///
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetTasks(int checklistId)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Return all tasks
            var taskEntities = await _repository.GetTasksAsync(checklistId);
            return Ok(_mapper.Map<IEnumerable<TaskItemDto>>(taskEntities));
        }

        /// <summary>
        /// Gets the task with ID taskId from the checklist with ID checklistId
        /// </summary>
        /// <param name="checklistId">The Id of the checklist</param>
        /// <param name="taskId">The Id of the task</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     GET /api/v1/checklists/1/tasks/1
        /// 
        /// </remarks>
        [HttpGet("{taskId}", Name="GetTask")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TaskItemDto>> GetTask(int checklistId, int taskId)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Ensure task exists 
            var taskEntity = await _repository.GetTaskAsync(checklistId, taskId);
            if (taskEntity == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<TaskItemDto>(taskEntity));
        }

        /// <summary>
        /// Creates a task in the checklist with ID checklistId
        /// </summary>
        /// <param name="checklistId">The Id of the checklist</param>
        /// <param name="taskItem"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/v1/checklists/1/tasks
        ///     {
        ///         "name": "new name",
        ///         "description": "new description",
        ///         "dueDate": "2022-09-01T03:39:25.993Z",
        ///         "priority": 0,
        ///         "isCompleted": false
        ///     }
        ///
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<TaskItemDto>> CreateTask(
            int checklistId, TaskItemForCreationDto taskItem)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Create the entity and save to repository
            var taskItemEntity = _mapper.Map<TaskItem>(taskItem);

            await _repository.AddTaskForChecklistAsync(checklistId, taskItemEntity);
            await _repository.SaveChangesAsync();


            // Returns a 201 Created
            var taskItemToReturn = _mapper.Map<TaskItemDto>(taskItemEntity);

            return CreatedAtRoute(
                "GetTask",
                new
                {
                    checklistId = checklistId,
                    taskId = taskItemToReturn.Id
                },
                taskItemToReturn);

        }

        /// <summary>
        /// Deletes the task with ID taskId from the checklist with ID checklistId
        /// </summary>
        /// <param name="checklistId">The ID of the checklist</param>
        /// <param name="taskId">THe ID of the task to be deleted</param>
        /// <remarks>
        /// Sample request:
        ///
        ///     DELETE /api/v1/checklists/1/tasks/1
        ///
        /// </remarks>
        [HttpDelete("{taskId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> DeleteTask(int checklistId, int taskId)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Ensure task to delete exists
            var taskToDelete = await _repository.GetTaskAsync(checklistId, taskId);
            if (taskToDelete == null)
            {
                return NotFound();
            }

            // Delete task
            await _repository.DeleteTaskAsync(taskToDelete);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Updates the task with ID taskId from the checklist with ID checklistId
        /// </summary>
        /// <param name="checklistId">The Id of the checklist</param>
        /// <param name="taskId">The Id of the task to be updated</param>
        /// <param name="task"></param>
        /// <remarks>
        /// Sample request:
        ///
        ///     PUT /api/v1/checklists/1/tasks/1
        ///     {
        ///         "name": "updated name",
        ///         "description": "updated description",
        ///         "dueDate": "2025-05-01T03:39:25.993Z",
        ///         "priority": 2,
        ///         "isCompleted": true
        ///     }
        /// 
        /// </remarks>
        [HttpPut("{taskId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> UpdateTask(
            int checklistId, int taskId, TaskItemForUpdateDto task)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Ensure task exists
            var taskEntity = await _repository.GetTaskAsync(checklistId, taskId);
            if (taskEntity == null)
            {
                return NotFound();
            }

            // Copy values over to the entity and save changes
            _mapper.Map(task, taskEntity);
            await _repository.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Partially updates the task with ID taskId from the checklist with ID checklistId
        /// </summary>
        /// <param name="checklistId"></param>
        /// <param name="taskId"></param>
        /// <param name="patchDocument"></param>
        /// <remarks>
        /// Note: See <a href="https://jsonpatch.com">Json patch</a> for more information.<br/>
        /// Sample request:
        /// 
        ///     PATCH /api/v1/checklists/1/tasks/1
        ///     [{
        ///         "op": "replace",
        ///         "path": "/description",
        ///         "value": "updated description" 
        ///     }]
        /// 
        /// </remarks>
        [HttpPatch("{taskId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PartiallyUpdateTask(
            int checklistId, int taskId, JsonPatchDocument<TaskItemForUpdateDto> patchDocument)
        {
            // Check if the user is allowed to access the checklist if it exists
            var user = await _userManager.GetUserAsync(User);
            if (!await _repository.ChecklistExistsAndByUserAsync(checklistId, user.Id))
            {
                return NotFound();
            }

            // Ensure task exists
            var taskEntity = await _repository.GetTaskAsync(checklistId, taskId);
            if (taskEntity == null)
            {
                return NotFound();
            }

            // Create temporary variable to apply the patch document
            var taskToPatch = _mapper.Map<TaskItemForUpdateDto>(taskEntity);
            patchDocument.ApplyTo(taskToPatch);

            // Check that validation attributes are satisfied after the patch
            if (!ModelState.IsValid || !TryValidateModel(taskToPatch))
            {
                return BadRequest(ModelState);
            }

            // Copy over values and save changes
            _mapper.Map(taskToPatch, taskEntity);
            await _repository.SaveChangesAsync();

            return NoContent();
        }
    }
}
