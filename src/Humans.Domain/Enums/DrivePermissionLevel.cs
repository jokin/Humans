namespace Humans.Domain.Enums;

/// <summary>
/// Google Drive permission role level for team resources.
/// Values match Google Drive API role names (lowercase) for easy mapping.
/// </summary>
public enum DrivePermissionLevel
{
    /// <summary>
    /// Read-only access to files.
    /// </summary>
    Reader = 0,

    /// <summary>
    /// Can view and add comments but not edit.
    /// </summary>
    Commenter = 1,

    /// <summary>
    /// Full read/write access to files.
    /// </summary>
    Writer = 2
}
