@echo off
chcp 65001 >nul
echo ================================
echo  ShopPro转TrMenu配置工具 - 运行
echo ================================
echo.

REM 检查可执行文件是否存在
if not exist "bin\Release\net472\ShopProToTrMenuConverter.exe" (
    echo 可执行文件不存在！
    echo 请先运行 build.bat 编译项目。
    pause
    exit /b 1
)

echo 正在运行转换工具...
echo.

bin\Release\net472\ShopProToTrMenuConverter.exe

echo.
pause
