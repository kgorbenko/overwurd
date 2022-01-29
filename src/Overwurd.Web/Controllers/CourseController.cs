using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Overwurd.Web.Controllers;

[UsedImplicitly]
public record CourseViewModel(int Id, string Name, string Description, DateTimeOffset CreatedAt);

[UsedImplicitly]
public record PaginatedCoursesViewModel(CourseViewModel[] Courses, int TotalCount);

[UsedImplicitly]
public record CreateCourseParameters(string Name, string Description);

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly ICourseRepository courseRepository;
    private readonly IReadOnlyCourseRepository readOnlyCourseRepository;
    private readonly ILogger<CourseController> logger;

    public CourseController([NotNull] UserManager<User> userManager,
                            [NotNull] ICourseRepository courseRepository,
                            [NotNull] IReadOnlyCourseRepository readOnlyCourseRepository,
                            [NotNull] ILogger<CourseController> logger)
    {
        if (userManager == null) throw new ArgumentNullException(nameof(userManager));
        if (readOnlyCourseRepository == null) throw new ArgumentNullException(nameof(readOnlyCourseRepository));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        this.readOnlyCourseRepository = readOnlyCourseRepository ?? throw new ArgumentNullException(nameof(readOnlyCourseRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Route("{id:int}")]
    public async Task<IActionResult> GetCourse(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var course = await readOnlyCourseRepository.FindByIdAsync(id, cancellationToken);

        if (course.User.Id != userId)
        {
            logger.LogInformation("User #{UserId} attempted to get Course #{CourseId}, but had no permission", userId, course.Id);
            return NotFound();
        }

        return Ok(MakeViewModel(course));
    }

    public async Task<IActionResult> PaginateUserCourses(int page, int pageSize, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var paginatedCourses = await readOnlyCourseRepository.PaginateUserCoursesAsync(userId: userId, page: page, pageSize: pageSize, cancellationToken);

        return Ok(MakePaginatedViewModel(paginatedCourses));
    }

    [Route("all")]
    public async Task<IActionResult> GetAllUserCourses(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var courses = await readOnlyCourseRepository.GetUserCoursesAsync(userId, cancellationToken);

        return Ok(courses.Select(MakeViewModel));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseParameters parameters, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);

        var course = new Course(name: parameters.Name, description: parameters.Description)
        {
            User = user
        };

        await courseRepository.AddAsync(course, cancellationToken);
        logger.LogInformation("User #{UserId} has successfully created Course #{CourseId}", user.Id, course.Id);

        return Ok(course.Id);
    }

    [HttpDelete]
    public async Task<IActionResult> RemoveCourse(int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var course = await courseRepository.FindByIdAsync(id, cancellationToken);

        if (course.User.Id != userId)
        {
            logger.LogInformation("User #{UserId} attempted to remove Course #{CourseId}, but had no permission", userId, course.Id);
            return NotFound();
        }

        if (course.Vocabularies.Any())
        {
            logger.LogInformation("User #{UserId} attempted to remove Course #{CourseId}, but that Course contains some vocabularies", userId, course.Id);
            return BadRequest($"Cannot remove Course #{id} because it contains vocabularies.");
        }

        await courseRepository.RemoveAsync(course, cancellationToken);
        logger.LogInformation("User #{UserId} successfully removed Course #{CourseId}", userId, course.Id);

        return NoContent();
    }

    private static CourseViewModel MakeViewModel(Course course) =>
        new(Id: course.Id,
            Name: course.Name,
            Description: course.Description,
            CreatedAt: course.CreatedAt);

    private static PaginatedCoursesViewModel MakePaginatedViewModel(PaginationResult<Course> paginationResult) =>
        new(
            paginationResult.Results.Select(MakeViewModel).ToArray(),
            paginationResult.TotalCount
        );
}