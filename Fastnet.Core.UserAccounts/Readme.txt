Using fastnet.Core.UserAccounts

This assembly provies a simple user account system - much less complex than IdentityServer (but, of course, does much less!).

The idea is that the database used here is the base db for other projects to use. The provided UserAccountDb dbcontext becomes a 'window' on
some database that has the necessary schema - which it will have if you start with the database you can create here.

To create such an emoty database, run the following command in powershell with the current directory set to C:\devroot\Fastnet.Core.UserAccounts:

dotnet ef database update

This will create a new databse in C:\devroot\Core\Fastnet.Core.UserAccounts\Data called UserAccounts.mdf.

Use this a start for your project (by copying, renaming, setting sql server logins, etc)

Note to asim:

The schema can be modified in the normal way by changing UsserAccountDb. To keep it simple, delete any current database existing sql server db called userAccounts and the
corresponding files in the Data folder (just delete the db in Sql management Studio). Delete the Migrations folder.

Then create a new migration by running:

dotnet ef migrations add InitialCreate

DO NOT use the package mabager as this requires a seperate startup project.

You can then create the db by running dotnet ef database update (as above)