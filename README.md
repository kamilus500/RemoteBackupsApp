# Remote Backups App

The title application is for keep user's backups remotely in database. Every backup is encrypted in sha256.

# How it work ?

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

# What you can do ?

- You can create new account or Login to existing.
- If you're logged you can upload file as backup and download from database.
