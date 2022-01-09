using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserPermission
{
    public enum PermissionsLevel
    {
        NORMAL,
        MODERATOR,
        OPERATOR
    };

    private PermissionsLevel permissionLevel = PermissionsLevel.NORMAL;

    public void setUserPermission(PermissionsLevel level)
    {
        permissionLevel = level;
    }

    public PermissionsLevel getPermissionLevel()
    {
        return permissionLevel;
    }

}
