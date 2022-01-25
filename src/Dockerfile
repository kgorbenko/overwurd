FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_NUMBER

RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - \
    && apt-get install -y nodejs

WORKDIR /src
COPY *.props .
COPY *.sln .
COPY Overwurd.Web/Overwurd.Web.csproj Overwurd.Web/
COPY Overwurd.Web.Tests/Overwurd.Web.Tests.csproj Overwurd.Web.Tests/
COPY Overwurd.Model/Overwurd.Model.csproj Overwurd.Model/
COPY Overwurd.Model.Tests/Overwurd.Model.Tests.csproj Overwurd.Model.Tests/
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
RUN dotnet publish Overwurd.Web/Overwurd.Web.csproj -c Release -o /publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS run
WORKDIR /app
COPY --from=publish /publish .
ENV ASPNETCORE_ENVIRONMENT="Production"
ENTRYPOINT ["dotnet", "Overwurd.Web.dll"]