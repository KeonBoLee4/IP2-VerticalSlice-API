namespace Api.Features.Courses;

using Application.Features.Courses.DeleteCourse;
using Api.Common.Extensions;
using Application.Abstractions.Messaging;
using Application.Features.Courses.CreateCourse;
using Application.Features.Courses.GetAllCourses;
using Application.Features.Courses.GetCourseById;
using Application.Features.Courses.UpdateCourse;
using Domain.Common;

public static class CourseEndpoints
{
    private const string GetByIdRouteName = "GetCourseById";

    public static void MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/courses")
            .WithTags("Courses");

        // I added this endpoint to get all available courses.
        group.MapGet("/", GetAll)
            .WithName("GetAllCourses")
            .WithSummary("Get all courses");

        // I added this endpoint to create a new course.
        group.MapPost("/", Create)
            .WithName("CreateCourse")
            .WithSummary("Create a new course");

        // I added this endpoint to get a specific course using its ID.
        group.MapGet("/{id:guid}", GetById)
            .WithName(GetByIdRouteName)
            .WithSummary("Get course by ID");

        // I added this PUT endpoint to update an existing course.
        group.MapPut("/{id:guid}", Update)
            .WithName("UpdateCourse")
            .WithSummary("Update an existing course");

        group.MapDelete("/{id:guid}", Delete)
            .WithName("DeleteCourse")
            .WithSummary("Delete a course");
    }

    // I created this method to handle course update requests.
    private static async Task<IResult> Update(
        Guid id,
        UpdateCourseRequest request,
        ICommandHandler<UpdateCourseCommand, Result<UpdateCourseResponse>> handler,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCourseCommand(
            id,
            request.Title,
            request.Code,
            request.Credits);

        var result = await handler.HandleAsync(command, cancellationToken);

        return result.Match(
            onSuccess: value => TypedResults.Ok(value),
            onFailure: _ => result.ToProblemDetails());
    }

    // I used this method to retrieve all courses from the database.
    private static async Task<IResult> GetAll(
        IQueryHandler<GetAllCoursesQuery, Result<GetAllCoursesResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetAllCoursesQuery();
        var result = await handler.HandleAsync(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblemDetails();
    }

    // I used this method to handle requests for creating a new course.
    private static async Task<IResult> Create(
        CreateCourseCommand command,
        ICommandHandler<CreateCourseCommand, Result<CreateCourseResponse>> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(command, cancellationToken);

        return result.IsSuccess
            ? TypedResults.CreatedAtRoute(
                result.Value,
                GetByIdRouteName,
                new { id = result.Value!.Id })
            : result.ToProblemDetails();
    }

    // I created this method to retrieve a course using its unique ID.
    private static async Task<IResult> GetById(
        Guid id,
        IQueryHandler<GetCourseByIdQuery, Result<GetCourseByIdResponse>> handler,
        CancellationToken cancellationToken)
    {
        var query = new GetCourseByIdQuery(id);
        var result = await handler.HandleAsync(query, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> Delete(
        Guid id,
        ICommandHandler<DeleteCourseCommand, Result> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new DeleteCourseCommand(id),
            cancellationToken);

        return result.Match(
            onSuccess: () => TypedResults.NoContent(),
            onFailure: _ => result.ToProblemDetails());
    }
}

// I created this request record to receive the updated course information.
public sealed record UpdateCourseRequest(
    string Title,
    string Code,
    int Credits);