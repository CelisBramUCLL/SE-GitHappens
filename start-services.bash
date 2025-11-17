#!/bin/bash

echo "Starting SE-GitHappens Real-time Services..."
echo

# === Start Backend ===
echo "Starting Backend..."
(
  cd "$(dirname "$0")/backend" || {
    echo "Backend folder not found."
    exit 1
  }
  dotnet run
) &

sleep 3

# === Start Frontend ===
echo "Starting Frontend..."
(
  cd "$(dirname "$0")/frontend" || {
    echo "Frontend folder not found."
    exit 1
  }
  npm install
  npm run dev
) &

sleep 3

echo
echo "Services are starting..."
echo "Backend: https://localhost:7159 or http://localhost:5097"
echo "Frontend: http://localhost:5173"
echo "Test Page: http://localhost:5097/realtime-test.html"
