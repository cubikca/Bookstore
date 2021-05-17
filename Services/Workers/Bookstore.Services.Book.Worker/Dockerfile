FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Services/Bookstore.Services.Book/Bookstore.Services.Book.csproj", "Services/Bookstore.Services.Book/"]
COPY ["Entities/Bookstore.Entities.Book/Bookstore.Entities.Book.csproj", "Entities/Bookstore.Entities.Book/"]
COPY ["Services/Workers/Bookstore.Services.Book.Worker/Bookstore.Services.Book.Worker.csproj", "Services/Workers/Bookstore.Services.Book.Worker/"]
RUN dotnet restore "Services/Workers/Bookstore.Services.Book.Worker/Bookstore.Services.Book.Worker.csproj"
COPY . .
WORKDIR "/src/Services/Workers/Bookstore.Services.Book.Worker"
RUN dotnet build "Bookstore.Services.Book.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bookstore.Services.Book.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bookstore.Services.Book.Worker.dll"]
