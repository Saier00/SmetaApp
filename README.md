# SmetaApp
SmetaApp is application using `ASP.NET MVC 5` and `Entity Framework 6` designed for processing cost estimates' data.

## Views
The views description and permisions are shown on the following schema:

![Imgur](https://i.imgur.com/uvU3mOg.png)


## Models
Application's data has following structure:

![Imgur](https://imgur.com/13FEJU6.png)

## Testing
Initializer JobInitializersTesting is used for seed the DB with jobs for testing.
SeedUsers method of RolesUsersInitializer class is used for seed the DB with users including superuser account which has all role's permissions.

## TODO
- Add opportunity to create, change and use table's templates for UDJobs and ListJobs views.
- Add validation errors' alerts.
- Remove deleted links inside the navigation bar.