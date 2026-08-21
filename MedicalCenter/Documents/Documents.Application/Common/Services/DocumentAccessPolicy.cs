using Common.Abstractions.Security;
using Documents.Application.Common.Behaviors;
using Documents.Domain;
using Documents.Domain.Constants;

namespace Documents.Application.Common.Services;

public static class DocumentAccessPolicy
{
    public static bool CanRead(StoredDocument document, CurrentUser? user)
    {
        if (document.IsPublic)
            return true;

        if (user is null || !user.IsAuthenticated)
            return false;

        if (document.OwnerProfileId.HasValue && document.OwnerProfileId == user.ProfileId)
            return true;

        return PermissionCheck.Has(user, Permissions.ViewPrivateDocuments);
    }

    public static bool CanDelete(StoredDocument document, CurrentUser? user)
    {
        if (user is null || !user.IsAuthenticated)
            return false;

        if (document.OwnerProfileId.HasValue && document.OwnerProfileId == user.ProfileId)
            return true;

        return PermissionCheck.Has(user, Permissions.DeleteDocuments);
    }
}
