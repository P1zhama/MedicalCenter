using ErrorOr;
using Grpc.Core;

namespace Profiles.API.ErrorMapping;

public static class ErrorMappingExtensions
{
    public static RpcException ToRpcException(this List<Error> errors)
    {
        var statusCode = errors[0].Type switch
        {
            ErrorType.Validation => StatusCode.InvalidArgument,
            ErrorType.NotFound => StatusCode.NotFound,
            ErrorType.Conflict => StatusCode.AlreadyExists,
            ErrorType.Unauthorized => StatusCode.Unauthenticated,
            ErrorType.Forbidden => StatusCode.PermissionDenied,
            _ => StatusCode.Internal
        };

        var detail = string.Join("; ", errors.Select(error => error.Description));

        return new RpcException(new Status(statusCode, detail));
    }
}
