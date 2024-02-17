# Remote Backups App

The title application is for keep user's backups remotely in database. Every backup is encrypted in sha256.

Web app is can use polish and english language

# Set configuration section

Go to MVC project and find file named appsettings.json.
  - Set up smpt settings for sending email.
  - Add connectionString

You can create image container from dockerfile and run application as docker container.

# How it works ?

Architecture of app is simple.
It has:
- soa services (Interfaces and Classes) which can upload backups, send emails, decrypt/encrypt data.
- own authentication system

# Tech stack 

- NET 7 MVC
- Bootstrap 5
- Jquery
- Sql Server
- Dapper
- Hangfire
- SignalR

# What you can do ?

- Create new account or Login to existing.
- If you're logged you can upload file as backup and download from database.
- As Admin you can ban every user who has not admin role
