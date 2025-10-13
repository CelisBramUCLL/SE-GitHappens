@echo off
echo Starting SE-GitHappens Real-time Services...
echo.

echo Starting Backend...
start "Backend" cmd /k "cd /d \"c:\Users\bramc\Documents\VU\Software Engineering I of II\SE-GitHappens\backend\" && dotnet run"

timeout /t 3 /nobreak > nul

echo Starting Frontend...
start "Frontend" cmd /k "cd /d \"c:\Users\bramc\Documents\VU\Software Engineering I of II\SE-GitHappens\frontend\" && npm run dev"

timeout /t 3 /nobreak > nul

echo.
echo Services are starting...
echo Backend: https://localhost:7159 or http://localhost:5097
echo Frontend: http://localhost:5173
echo Test Page: http://localhost:5097/realtime-test.html
echo.
echo Press any key to exit...
pause > nul