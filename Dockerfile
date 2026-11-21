FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["momentify-backend.csproj", "./"]
RUN dotnet restore "momentify-backend.csproj"

COPY . .
RUN dotnet publish "momentify-backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "momentify-backend.dll"]

