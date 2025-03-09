# Stage 1: Build project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /build

ARG COMMIT_HASH
COPY ["Directory.Build.props", "src", "."]
RUN dotnet restore CatsUdon.CharacterSheets.Web/CatsUdon.CharacterSheets.Web.csproj
RUN dotnet publish CatsUdon.CharacterSheets.Web/CatsUdon.CharacterSheets.Web.csproj -c Release -o out --self-contained false
RUN echo "$COMMIT_HASH" > out/commit_hash

# Stage 2: Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /build/out .
RUN ln -sf /usr/share/zoneinfo/Japan /etc/localtime
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "CatsUdon.CharacterSheets.Web.dll"]