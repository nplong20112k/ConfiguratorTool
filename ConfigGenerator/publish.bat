ECHO "Start publishing portable linux-x64"
dotnet publish .\ConfigGenerator\ConfigGenerator.csproj /p:Configuration=Release /p:PublishProfile=.\ConfigGenerator\Properties\PublishProfiles\linux-x64.pubxml

ECHO "Start publishing portable win-x64"
dotnet publish .\ConfigGenerator\ConfigGenerator.csproj /p:Configuration=Release /p:PublishProfile=.\ConfigGenerator\Properties\PublishProfiles\win-x64.pubxml