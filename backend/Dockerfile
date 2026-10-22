# ==============================================================================
# Dockerfile para Backend API (.NET 10)
# Sistema de Control de Caja Universitaria - TecNM Campus Monclova
# ==============================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Puerto HTTP estándar de ASP.NET Core
EXPOSE 8080

# Copiar artefactos compilados y publicados
COPY ./publish/ .

# Iniciar la aplicación
ENTRYPOINT ["dotnet", "TecNM.Api.dll"]
