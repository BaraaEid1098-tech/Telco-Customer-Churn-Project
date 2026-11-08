FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["BackEnd/Directory.Build.props", "BackEnd/"]
COPY ["BackEnd/Churn.API/Churn.API.csproj", "BackEnd/Churn.API/"]
COPY ["BackEnd/Churn.Application/Churn.Application.csproj", "BackEnd/Churn.Application/"]
COPY ["BackEnd/Churn.Domain/Churn.Domain.csproj", "BackEnd/Churn.Domain/"]
COPY ["BackEnd/Churn.Infrastructure/Churn.Infrastructure.csproj", "BackEnd/Churn.Infrastructure/"]
RUN dotnet restore "BackEnd/Churn.API/Churn.API.csproj"

COPY BackEnd/ BackEnd/
WORKDIR /src/BackEnd
RUN dotnet publish "Churn.API/Churn.API.csproj" \
	-c Release \
	-o /app/publish \
	--no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Churn.API.dll"]
