$ErrorActionPreference = "Stop"

dotnet restore ".\NextServer.sln"
dotnet test "Tests.Unit\Tests.Unit.csproj" -c Release -v normal --no-restore

dotnet publish ".\NextServer\NextServer.csproj" -r alpine-x64 --self-contained false -c "Release" -f "net5.0"
docker build -t "grayscale/next-chat-server:latest" ".\NextServer\bin\Release\net5.0\alpine-x64\publish\"

dotnet publish ".\Configurator\Configurator.csproj" -r alpine-x64 --self-contained false -c "Release" -f "net5.0"
docker build -t "grayscale/next-chat-configurator:latest" ".\Configurator\bin\Release\net5.0\alpine-x64\publish\"

dotnet publish ".\Tests.Auto\Tests.Auto.csproj" -r alpine-x64 --self-contained false -c "Release" -f "net5.0"
docker build -t "grayscale/next-chat-tests:latest" ".\Tests.Auto\bin\Release\net5.0\alpine-x64\publish\"
