# CodeCompanion.Extensions.Dapper.Postgres
[![GitHub release (latest SemVer)](https://img.shields.io/github/v/release/kblyr/CodeCompanion.Extensions.Dapper.Postgres?color=white&logo=github)](https://github.com/kblyr/CodeCompanion.Extensions.Dapper.Postgres)
[![Nuget version (CodeCompanion.Extensions.Dapper.Postgres)](https://img.shields.io/nuget/v/CodeCompanion.Extensions.Dapper.Postgres?logo=nuget)](https://www.nuget.org/packages/CodeCompanion.Extensions.Dapper.Postgres)

Dapper supports Multiple result set, but PostgreSQL didn't. PostgreSQL has **REFCURSOR** but Dapper doesn't support it. So this simple and small library fills the gap.

The library imitates the **QueryMultiple(...)** extension method provided by Dapper.

Presenting the **QueryRefcursors(...)** extension method. The method expects a **functionName**, an instance of **NpgsqlTransaction** and an optional **param**. In PostgreSQL, the function must return **SETOF REFCURSOR**, this is how we can imitate the multiple result set

## Example
In this example, the *User* has many *Roles* which has many *Permissions*

We wan't to make 1 database call (not really 1 call hehe :P) to fetch the following:
* Id and Username of the user
* Id and Name of Roles which the User has
* Id and Name of Permissions which the User has (we will traverse this using: UserRole.UserId = RolePermission.RoleId -> RolePermission.PermissionId -> Permission.Id)

### Database Structure
Assuming you have 5 tables in the database
* User
    * **Id**
    * **Username**
* Role
    * **Id**
    * **Name**
* Permission
    * **Id**
    * **Name**
* UserRole
    * **UserId** | FK: *User.Id*
    * **RoleId** | FK: *Role.Id*
* RolePermission
    * **RoleId** | FK: *Role.Id*
    * **PermissionId** | FK: *Permission.Id*

### PostgreSQL (PL/PGSQL)
``` sql
CREATE OR REPLACE FUNCTION "get_user_with_roles_and_permissions"("user__id" INTEGER)
RETURNS SETOF REFCURSOR AS
$BODY$
DECLARE
    -- Refcursor declarations
    "ref__user" REFCURSOR;
    "ref__roles" REFCURSOR;
    "ref__permissions" REFCURSOR;
BEGIN
    -- Select User
    -- NOTE: this only query for exactly 1 row
    OPEN "ref__user" FOR
    SELECT "User"."Id", "User"."Username"
    FROM "User"
    WHERE "User"."Id" = "user__id"
    LIMIT 1;
    RETURN NEXT "ref__user";

    -- Select Roles
    OPEN "ref__roles" FOR
    SELECT "Role"."Id", "Role"."Name"
    FROM "Role"
    INNER JOIN "UserRole" ON "Role"."Id" = "UserRole"."RoleId"
    WHERE "UserRole"."UserId" = "user__id";
    RETURN NEXT "ref__roles";

    -- Select Permissions
    -- NOTE: There's a chance that user has many roles which have same permission, we use DISTINCT to eliminate duplicates
    OPEN "ref__permissions" FOR
    SELECT DISTINCT "Permission"."Id", "Permission"."Name"
    FROM "Permission"
    INNER JOIN "RolePermission" ON "Permission"."Id" = "RolePermission"."PermissionId"
    INNER JOIN "UserRole" ON "RolePermission"."RoleId" = "UserRole"."RoleId"
    WHERE "UserRole"."UserId" = "user__id";
    RETURN NEXT "ref__permissions";
END;
$BODY$
```

### C#
``` csharp
/* Entity models */
record User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public IEnumerable<Role> Roles { get; set; }
    public IEnumerable<Permission> Permissions { get; set; }
}

record Role
{
    public int Id { get; set; }
    public string Name { get; set; }
}

record Permission
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// Calling the database function in C# code

// Create an instance of NpgsqlConnection
using var connection = new NpgsqlConnection(connectionString);

// Try to open a database connection
connection.Open();

// Begin a database transaction
using var transaction = connection.BeginTransaction();

// Query refcursors
// Call a function with name 'get_user_with_roles_and_permissions' with parameter 'user_id' = 1
var refcursors = connection.QueryRefcursor("get_user_with_roles_and_permissions", transaction, new { user__id = 1 });

// we use ReadSingleOrDefault because we're sure that there is only one user that has an id of 1 (or none if the user with id = 1 doesn't exists)
var user = refcursors.ReadSingleOrDefault<User>();

// Check if user with id = 1 exists
if (user is not null)
{
    // Query for roles
    user.Roles = refcursors.Read<Role>();

    // Query for permissions
    user.Permissions = refcursors.Read<Permission>(); 
}
```

### Conclusion
Just like the multiple result set, you should be aware of the order of refcursors in the database function
If you read in **Refcursors** more than the function returns, an exception of type **NoRefcursorLeftException**

## Download the Package
[View on NuGet.org](https://www.nuget.org/packages/CodeCompanion.Extensions.Dapper.Postgres/)
### .NET CLI
``` powershell
dotnet add package CodeCompanion.Extensions.Dapper.Postgres --version 1.0.0-pre
```
### Package Reference (.csproj)
``` xml
<PackageReference Include="CodeCompanion.Extensions.Dapper.Postgres" Version="1.0.0-pre" />
```
### Package Manager
``` powershell
Install-Package CodeCompanion.Extensions.Dapper.Postgres -Version 1.0.0-pre
```
