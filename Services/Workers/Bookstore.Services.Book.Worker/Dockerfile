#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Services/Workers/Bookstore.Services.Book.Worker/Bookstore.Services.Book.Worker.csproj", "Services/Workers/Bookstore.Services.Book.Worker/"]
COPY ["Entities/Bookstore.Entities.Book/Bookstore.Entities.Book.csproj", "Entities/Bookstore.Entities.Book/"]
COPY ["Domains/Bookstore.Domains.Book/Bookstore.Domains.Book.csproj", "Domains/Bookstore.Domains.Book/"]
COPY ["Domains/Bookstore.Domains.People/Bookstore.Domains.People.csproj", "Domains/Bookstore.Domains.People/"]
COPY ["Services/Bookstore.Services.Book/Bookstore.Services.Book.csproj", "Services/Bookstore.Services.Book/"]
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