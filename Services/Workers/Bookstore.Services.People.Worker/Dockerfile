FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Services/Bookstore.Services.People/Bookstore.Services.People.csproj", "Services/Bookstore.Services.People/"]
COPY ["Entities/Bookstore.Entities.People/Bookstore.Entities.People.csproj", "Entities/Bookstore.Entities.People/"]
COPY ["Services/Workers/Bookstore.Services.People.Worker/Bookstore.Services.People.Worker.csproj", "Services/Workers/Bookstore.Services.People.Worker/"]
RUN dotnet restore "Services/Workers/Bookstore.Services.People.Worker/Bookstore.Services.People.Worker.csproj"
COPY . .
WORKDIR "/src/Services/Workers/Bookstore.Services.People.Worker"
RUN dotnet build "Bookstore.Services.People.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bookstore.Services.People.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bookstore.Services.People.Worker.dll"]
