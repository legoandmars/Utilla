$Name = "Utilla"
dotnet build -c Release
mkdir BepInEx\plugins\$Name
cp bin\Release\netstandard2.0\$Name.dll BepInEx\plugins\$Name\
Compress-Archive .\BepInEx\ $Name-v
rmdir .\BepInEx\ -Recurse