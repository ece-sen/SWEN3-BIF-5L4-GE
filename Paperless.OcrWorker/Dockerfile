FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY Paperless.OcrWorker/Paperless.OcrWorker.csproj Paperless.OcrWorker/
RUN dotnet restore Paperless.OcrWorker/Paperless.OcrWorker.csproj

COPY Paperless.OcrWorker/. Paperless.OcrWorker/
WORKDIR /src/Paperless.OcrWorker
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final

RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-eng \
    ghostscript \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Paperless.OcrWorker.dll"]
