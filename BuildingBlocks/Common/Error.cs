namespace Shared.Services.BuildingBlocks.Common;

public class Error
{
    public static Error NotFound(string? message = "The requested resource was not found.") => new Error(message ?? "The requested resource was not found.");
    public static Error BadRequest(string? message = "The request could not be understood or was missing required parameters.") => new Error(message = "The request could not be understood or was missing required parameters.");
    public static Error Unauthorized(string? message = "You are not authorized to access this resource.") => new Error(message ?? "You are not authorized to access this resource.");
    public static Error Forbidden(string? message = "Access to this resource is forbidden.") => new Error(message ?? "Access to this resource is forbidden.");
    public static Error Conflict(string? message = "There is a conflict with the current state of the resource.") => new Error(message ?? "There is a conflict with the current state of the resource.");
    public static Error InternalServerError(string? message = "An unexpected error occurred on the server.") => new Error(message ?? "An unexpected error occurred on the server.");
    private Error(string message)
    {
        ErrorMessage = message;
    }

    public string ErrorMessage { get; private set; }
}