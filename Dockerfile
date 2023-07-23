FROM mcr.microsoft.com/dotnet/sdk:7.0-bullseye-slim AS build-env
RUN apt-get update && apt-get install -y libfontconfig1 libfontconfig1-dev
WORKDIR /App

# Copy everything
COPY . ./
# Copy fonts
RUN mkdir ~/.fonts
RUN cp -r fonts/*.ttf ~/.fonts
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-bullseye-slim
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "IBCVideoCourseTGBot.dll"]