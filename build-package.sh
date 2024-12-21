dotnet build -c Release src/vk.Generator/vk.Generator.csproj
dotnet build -c Release src/vk.Rewrite/vk.Rewrite.csproj
dotnet pack -c Release src/vk/vk.csproj
