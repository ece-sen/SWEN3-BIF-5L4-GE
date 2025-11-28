# ===========================
# Build Stage
# ===========================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj
COPY Paperless.OcrWorker.csproj ./
RUN dotnet restore "./Paperless.OcrWorker.csproj"

# Copy all files
COPY . ./
RUN dotnet publish "./Paperless.OcrWorker.csproj" -c Release -o /app/publish /p:UseAppHost=false


# ===========================
# Runtime Stage
# ===========================
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Install OCR dependencies
RUN apt-get update && apt-get install -y \
    tesseract-ocr \
    tesseract-ocr-eng \
    ghostscript \
    && rm -rf /var/lib/apt/lists/*

# Copy published worker
COPY --from=build /app/publish .

# Tesseract data path (Container path)
ENV TESSDATA_PREFIX=/usr/share/tesseract-ocr/4.00/tessdata

EXPOSE 8082
ENTRYPOINT ["dotnet", "Paperless.OcrWorker.dll"]
