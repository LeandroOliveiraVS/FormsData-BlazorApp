# Estágio 1
# Usamos uma imagem oficial da Microsoft que já tem o .NET SDK completo.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os ficheiros do projeto (.csproj) e restaura as dependências (pacotes NuGet).
COPY ["FormulariosData.csproj", "."]

# Restaura as dependências (pacotes NuGet)
RUN dotnet restore "./FormulariosData.csproj"

# Copia o restante do código-fonte.
COPY . .
WORKDIR "/src/."

# Publica a aplicação numa versão otimizada para produção
RUN dotnet publish "FormulariosData.csproj" -c Release -o /app/publish

# Estágio 2
# Usamos uma imagem muito mais leve que só tem o necessário para executar a aplicação.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Copia a aplicação já compilada do estágio de "build" para a nossa imagem final
COPY --from=build /app/publish .

# Expõe a porta 8080 para o mundo exterior do contêiner
EXPOSE 8080
# Define o comando que será executado quando o contêiner iniciar
ENTRYPOINT ["dotnet", "FormulariosData.dll"]