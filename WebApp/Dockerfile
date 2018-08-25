FROM launcher.gcr.io/google/aspnetcore
ADD ./ /app
ENV ASPNETCORE_URLS=http://*:${PORT}
WORKDIR /app
ENTRYPOINT [ "dotnet", "WebApp.dll" ]