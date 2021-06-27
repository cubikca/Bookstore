#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["UI/Web/Bookstore.UI.Web.Store/Server/Bookstore.UI.Web.Store.Server.csproj", "UI/Web/Bookstore.UI.Web.Store/Server/"]
COPY ["UI/Web/Bookstore.UI.Web.Store/Client/Bookstore.UI.Web.Store.Client.csproj", "UI/Web/Bookstore.UI.Web.Store/Client/"]
COPY ["UI/Web/Bookstore.UI.Web.Store/Shared/Bookstore.UI.Web.Store.Shared.csproj", "UI/Web/Bookstore.UI.Web.Store/Shared/"]
RUN dotnet restore "UI/Web/Bookstore.UI.Web.Store/Server/Bookstore.UI.Web.Store.Server.csproj"
COPY . .
WORKDIR "/src/UI/Web/Bookstore.UI.Web.Store/Server"
RUN dotnet build "Bookstore.UI.Web.Store.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bookstore.UI.Web.Store.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bookstore.UI.Web.Store.Server.dll"]