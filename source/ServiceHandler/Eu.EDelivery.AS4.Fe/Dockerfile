FROM microsoft/aspnetcore:1.0.1
ENTRYPOINT ["dotnet", "Eu.EDelivery.AS4.Fe.dll"]
ARG source=.
WORKDIR /app
EXPOSE 80
COPY $source .
