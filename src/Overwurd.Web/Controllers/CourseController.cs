using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;

namespace Overwurd.Web.Controllers;

[UsedImplicitly]
public record CourseViewModel(int Id, string Name, string Description, DateTimeOffset CreatedAt);

[UsedImplicitly]
public record PaginatedCoursesViewModel(CourseViewModel[] Courses, int TotalCount);

[UsedImplicitly]
public record CourseParameters(string Name, string Description);

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly UserManager<User> userManager;
    private readonly ICourseRepository courseRepository;
    private readonly IReadOnlyCourseRepository readOnlyCourseRepository;
    private readonly IReadOnlyVocabularyRepository readOnlyVocabularyRepository;
    private readonly ILogger<CourseController> logger;

    public CourseController([NotNull] UserManager<User> userManager,
                            [NotNull] ICourseRepository courseRepository,
                            [NotNull] IReadOnlyCourseRepository readOnlyCourseRepository,
                            [NotNull] IReadOnlyVocabularyRepository readOnlyVocabularyRepository,
                            [NotNull] ILogger<CourseController> logger)
    {
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.courseRepository = courseRepository ?? throw new ArgumentNullException(nameof(courseRepository));
        this.readOnlyCourseRepository = readOnlyCourseRepository ?? throw new ArgumentNullException(nameof(readOnlyCourseRepository));
        this.readOnlyVocabularyRepository = readOnlyVocabularyRepository ?? throw new ArgumentNullException(nameof(readOnlyVocabularyRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [Route("getByName")]
    public async Task<IActionResult> GetCourseByName([BindRequired] string name, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var course = await readOnlyCourseRepository.GetUserCourseByNameAsync(userId, name, cancellationToken);

        if (course is null)
        {
            return NotFound();
        }

        return Ok(MakeViewModel(course));
    }

    [HttpGet]
    [Route("{id:int}")]
    public async Task<IActionResult> GetCourseById([BindRequired] int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var course = await readOnlyCourseRepository.FindByIdAsync(id, cancellationToken);

        if (course.UserId != userId)
        {
            logger.LogWarning("User #{UserId} attempted to get Course #{CourseId}, but had no permission", userId, course.Id);
            return NotFound();
        }

        return Ok(MakeViewModel(course));
    }

    [HttpGet]
    [Route("paginate")]
    public async Task<IActionResult> PaginateUserCourses([BindRequired] int page, [BindRequired] int pageSize, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var paginatedCourses = await readOnlyCourseRepository.PaginateUserCoursesAsync(userId: userId, page: page, pageSize: pageSize, cancellationToken);

        return Ok(MakePaginatedViewModel(paginatedCourses));
    }

    [HttpGet]
    [Route("all")]
    public async Task<IActionResult> GetAllUserCourses(CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var courses = await readOnlyCourseRepository.GetUserCoursesAsync(userId, cancellationToken);

        return Ok(courses.Select(MakeViewModel));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody, BindRequired] CourseParameters parameters, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);

        var course = new Course(name: parameters.Name, description: parameters.Description)
        {
            User = user
        };

        var existingCourse = await readOnlyCourseRepository.GetUserCourseByNameAsync(user.Id, course.Name, cancellationToken);
        if (existingCourse is not null)
        {
            return Conflict();
        }

        await courseRepository.AddAsync(course, cancellationToken);
        logger.LogInformation("User #{UserId} has successfully created Course #{CourseId}", user.Id, course.Id);

        return Ok(course.Id);
    }

    [HttpPut]
    [Route("{id:int}")]
    public async Task<IActionResult> UpdateCourse([BindRequired] int id, [FromBody, BindRequired] CourseParameters parameters, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var course = await courseRepository.FindByIdAsync(id, cancellationToken);

        if (course.UserId != userId)
        {
            logger.LogWarning("User #{UserId} attempted to update Course #{CourseId}, but had not permission", userId, course.Id);
            return NotFound();
        }

        var existingCourse = await readOnlyCourseRepository.GetUserCourseByNameAsync(userId, course.Name, cancellationToken);
        if (existingCourse is not null)
        {
            return Conflict();
        }

        course.Name = parameters.Name;
        course.Description = parameters.Description;

        await courseRepository.UpdateAsync(course, cancellationToken);
        logger.LogInformation("User #{UserId} successfully updated Course #{CourseId}", userId, course.Id);

        return NoContent();
    }

    [HttpDelete]
    [Route("{id:int}")]
    public async Task<IActionResult> RemoveCourse([BindRequired] int id, CancellationToken cancellationToken)
    {
        var userId = userManager.GetUserId(User).ParseUserId();
        var course = await courseRepository.FindByIdAsync(id, cancellationToken);

        if (course.UserId != userId)
        {
            logger.LogWarning("User #{UserId} attempted to remove Course #{CourseId}, but had no permission", userId, course.Id);
            return NotFound();
        }

        var vocabularyCount = await readOnlyVocabularyRepository.CountCourseVocabulariesAsync(course.Id, cancellationToken);
        if (vocabularyCount > 0)
        {
            return Conflict();
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