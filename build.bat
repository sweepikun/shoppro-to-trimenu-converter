@echo off
chcp 65001 >nul
echo ================================
echo  ShopPro转TrMenu配置工具 - 编译
echo ================================
echo.

REM 检查.NET CLI是否可用
where dotnet >nul 2>nul
if errorlevel 1 (
    echo 错误: 未找到 dotnet 命令
    echo 请安装 .NET Framework SDK 或使用 Visual Studio 编译
    pause
    exit /b 1
)

echo 正在还原NuGet包...
dotnet restore

echo.
echo 正在编译项目...
dotnet build --configuration Release

if errorlevel 1 (
    echo.
    echo 编译失败！
    pause
    exit /b 1
)

echo.
echo 编译成功！可执行文件位于: bin\Release\net472\ShopProToTrMenuConverter.exe
pause
