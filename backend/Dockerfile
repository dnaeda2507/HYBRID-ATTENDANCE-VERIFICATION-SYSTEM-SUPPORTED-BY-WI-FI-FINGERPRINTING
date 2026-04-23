# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY CleanArchitecture.sln .
COPY CleanArchitecture/CleanArchitecture.WebApi/CleanArchitecture.WebApi.csproj         CleanArchitecture/CleanArchitecture.WebApi/
COPY CleanArchitecture/CleanArchitecture.Infrastructure/CleanArchitecture.Infrastructure.csproj  CleanArchitecture/CleanArchitecture.Infrastructure/
COPY CleanArchitecture/CleanArchitecture.Application/CleanArchitecture.Core.csproj      CleanArchitecture/CleanArchitecture.Application/

RUN dotnet restore "CleanArchitecture/CleanArchitecture.WebApi/CleanArchitecture.WebApi.csproj"

COPY CleanArchitecture/ CleanArchitecture/

RUN dotnet publish "CleanArchitecture/CleanArchitecture.WebApi/CleanArchitecture.WebApi.csproj" \
    -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CleanArchitecture.WebApi.dll"]
