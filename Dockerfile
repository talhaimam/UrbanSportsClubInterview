# BUILD IMAGE
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /build
COPY . .
RUN dotnet restore
RUN dotnet build
RUN dotnet publish ./Source/InterviewService.csproj -c Release -o Release

# RUNTIME IMAGE
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime
WORKDIR /app
COPY --from=build /build/Release ./
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER={846F5F1C-F9AE-4B07-969E-05C26BC060D8}
ENV CORECLR_PROFILER_PATH=/opt/datadog/Datadog.Trace.ClrProfiler.Native.so
ENV DD_INTEGRATIONS=/opt/datadog/integrations.json
ENV DD_DOTNET_TRACER_HOME=/opt/datadog
RUN curl -LO https://github.com/DataDog/dd-trace-dotnet/releases/download/v1.19.2/datadog-dotnet-apm_1.19.2_amd64.deb && dpkg -i ./datadog-dotnet-apm_1.19.2_amd64.deb
ENTRYPOINT ["dotnet", "InterviewService.dll"]
