# kanson-netcore-backend
Kanson - A Kanban Board with Masonry layout

![image](https://user-images.githubusercontent.com/25080659/83320424-fe55cd80-a279-11ea-9ae9-fc291120df55.png)

Source code: 

Frontend - https://github.com/tyrng/kanson-react-frontend

Backend - https://github.com/tyrng/kanson-netcore-backend

Hosted at: https://kanson.herokuapp.com/

# Features
- Masonry layout!
- Easy drag and drop like any Kanban Board
- Able to view all lists and cards in one page
- Fast and lightweight

# Backend Requirements
- SQL Server 2019
- .NET Core 3.0

# Setting Up
1. Create a new schema in SQL Server and run the provided database query script.
2. Change the connection strings in appsettings.json.
3. (Optional) change the "Secret" property in appsettings.json, it is the secret key in the JWT authentication token.
