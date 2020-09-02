@echo off
nuget pack -Build -Properties Configuration=Release .\Kw.Common\Kw.Common.csproj
nuget pack -Build -Properties Configuration=Release .\Kw.Aspects\Kw.Aspects.csproj
nuget pack -Build -Properties Configuration=Release .\Kw.WinAPI\Kw.WinAPI.csproj
nuget pack -Build -Properties Configuration=Release .\Kw.Windows\Kw.Windows.csproj
nuget pack -Build -Properties Configuration=Release .\Kw.Windows.Forms\Kw.Windows.Forms.csproj
