# multi stage build, the sdk image compiles and publishes, the smaller aspnet image runs it
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy the project file and restore first so this layer is cached when only code changes
COPY *.csproj ./
RUN dotnet restore

# now copy the rest and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
# make the uploads folder and hand everything to the built in non root app user before dropping privileges
RUN mkdir -p wwwroot/uploads/contracts && chown -R app:app /app
USER app
ENTRYPOINT ["dotnet", "TechMove.Api.dll"]
