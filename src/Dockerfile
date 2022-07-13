FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_NUMBER

RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - \
    && apt-get install -y nodejs

WORKDIR /src
COPY *.props .
COPY *.sln .
COPY Overwurd.Domain/Overwurd.Domain.fsproj Overwurd.Domain/
COPY Overwurd.Domain.Tests/Overwurd.Domain.Tests.fsproj Overwurd.Domain.Tests/
COPY Overwurd.Infrastructure/Overwurd.Infrastructure.fsproj Overwurd.Infrastructure/
COPY Overwurd.Infrastructure.Tests/Overwurd.Infrastructure.Tests.fsproj Overwurd.Infrastructure.Tests/
COPY Overwurd.Web/Overwurd.Web.fsproj Overwurd.Web/
RUN dotnet restore

COPY . .
RUN dotnet build -c Release -p:BuildNumber=$BUILD_NUMBER --no-restore

FROM build as test
ARG CONNECTION_STRING
ENV ConnectionStrings__DefaultTest=$CONNECTION_STRING
WORKDIR /src
RUN dotnet test -c Release --no-build

FROM test AS publish
WORKDIR /src
RUN dotnet publish Overwurd.Web/Overwurd.Web.fsproj -c Release -o /publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS run
WORKDIR /app
COPY --from=publish /publish .
ENV ASPNETCORE_ENVIRONMENT="Production"
ENTRYPOINT ["dotnet", "Overwurd.Web.dll"]