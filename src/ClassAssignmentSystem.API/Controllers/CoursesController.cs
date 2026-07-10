using ClassAssignmentSystem.Application.Features.Courses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClassAssignmentSystem.API.Controllers
{
    [Authorize]
    [Route("api/courses")]
    public class CoursesController : BaseApiController
    {
        private readonly IMediator _mediator;
        public CoursesController(IMediator mediator) => _mediator = mediator;

        /// <summary>List all courses with available seat counts.</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct)
            => Ok(await _mediator.Send(new GetAllCoursesQuery(), ct));

        /// <summary>Get a specific course by ID.</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            var course = await _mediator.Send(new GetCourseByIdQuery(id), ct);
            return course is null ? NotFound() : Ok(course);
        }
    }
}
