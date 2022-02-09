#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM  mcr.microsoft.com/dotnet/sdk:6.0
COPY dist /app
WORKDIR /app
VOLUME /app/wwwwroot
VOLUME /app/nlog
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "FluUrl.dll"]