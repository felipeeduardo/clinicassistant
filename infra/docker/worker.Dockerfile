FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/ .
RUN dotnet restore ClinicAssistant.sln
RUN dotnet publish src/ClinicAssistant.Worker/ClinicAssistant.Worker.csproj --configuration Release --no-restore --output /app/publish

# The worker uses shared ASP.NET Core libraries through its hosting and logging stack.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ClinicAssistant.Worker.dll"]
