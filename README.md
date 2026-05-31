# TechMove - PROG7311 Part 3

This is my Part 3 for PROG7311. In Part 2 everything sat in one MVC project. For Part 3 I split it up so all the database work happens in a separate Web API, and the MVC site just calls that api over http. There is also a docker compose file so the whole thing can run together in containers.

## The projects

- TechMove.Api - the web api. this is the only project that touches the database. it has the ef core context, the design patterns from part 1 (factory, observer, strategy), the services, the jwt login and swagger.
- TechMove.Web - the mvc site. it has no database of its own, it gets everything from the api with httpclient and keeps the login token in session.
- TechMove.Tests - the xunit tests, the normal unit tests plus some integration tests that actually call the api.

## Running it in Visual Studio

You have to run the api and the web project at the same time, otherwise the site cant reach the api and you just get an "unavailable" page. Right click the solution, choose set startup projects, pick multiple startup projects and put TechMove.Api and TechMove.Web both on Start.

The api runs on https://localhost:7257 and swagger opens by itself. The web site already knows that address from its appsettings.json. First time you run it you might need to do Update-Database in the package manager console, or just let the api make the database when it starts.

## Running it with docker

From the folder the solution is in:

    docker compose up --build

That brings up three containers, the sql server, the api and the web site. The site comes up on http://localhost:5000 and the api swagger on http://localhost:5080/swagger. The containers find each other by name so the web reaches the api on http://techmove-api:8080.

## Logging in

You can look at all the list and details pages without logging in. To create, edit, delete or change a status you have to log in first. The account is:

username: admin
password: Admin123!

## Tests

    dotnet test TechMove.sln

They also run on github actions every time i push, the workflow file is in .github/workflows.

## The api endpoints

the main ones are:

- GET /api/contracts (can filter by from, to and status)
- POST /api/contracts, PUT /api/contracts/{id}, PATCH /api/contracts/{id}/status
- POST /api/contracts/{id}/agreement for the signed pdf
- the clients and servicerequests endpoints work the same kind of way
- POST /api/auth/login to get the token

my reflection report is in the docs folder.
