#!/bin/bash

DIR="$(cd "$(dirname "$0")" && pwd)"
PIDFILE_BACKEND="/tmp/40kdb-backend.pid"
PIDFILE_FRONTEND="/tmp/40kdb-frontend.pid"
LOG_BACKEND="/tmp/40kdb-backend.log"
LOG_FRONTEND="/tmp/40kdb-frontend.log"

start() {
    echo "Starting 40KDB..."

    if [ -f "$PIDFILE_BACKEND" ] && kill -0 "$(cat $PIDFILE_BACKEND)" 2>/dev/null; then
        echo "Backend already running (PID $(cat $PIDFILE_BACKEND))"
    elif lsof -i :5050 -sTCP:LISTEN >/dev/null 2>&1; then
        echo "ERROR: Port 5050 already in use by another process:"
        lsof -i :5050 -sTCP:LISTEN
        exit 1
    else
        (cd "$DIR/backend/40kdb" && dotnet run --urls "http://localhost:5050") > "$LOG_BACKEND" 2>&1 &
        echo $! > "$PIDFILE_BACKEND"
        echo "Backend started (PID $!)"
    fi

    if [ -f "$PIDFILE_FRONTEND" ] && kill -0 "$(cat $PIDFILE_FRONTEND)" 2>/dev/null; then
        echo "Frontend already running (PID $(cat $PIDFILE_FRONTEND))"
    elif lsof -i :4000 -sTCP:LISTEN >/dev/null 2>&1; then
        echo "ERROR: Port 4000 already in use by another process:"
        lsof -i :4000 -sTCP:LISTEN
        exit 1
    else
        (cd "$DIR/frontend" && npm run dev) > "$LOG_FRONTEND" 2>&1 &
        echo $! > "$PIDFILE_FRONTEND"
        echo "Frontend started (PID $!)"
    fi

    echo ""
    echo "Backend:  http://localhost:5050"
    echo "Frontend: http://localhost:4000"
}

stop() {
    echo "Stopping 40KDB..."

    for ENTRY in "$PIDFILE_BACKEND:5050:Backend" "$PIDFILE_FRONTEND:4000:Frontend"; do
        IFS=: read -r PF PORT NAME <<< "$ENTRY"
        if [ -f "$PF" ]; then
            PID=$(cat "$PF")
            kill -9 "$PID" 2>/dev/null
            rm -f "$PF"
        fi
        PIDS=$(lsof -ti :"$PORT" -sTCP:LISTEN 2>/dev/null)
        if [ -n "$PIDS" ]; then
            echo "$PIDS" | xargs kill -9 2>/dev/null
            echo "$NAME stopped"
        else
            echo "$NAME not running"
        fi
    done
}

status() {
    echo "40KDB Status:"
    echo ""

    if [ -f "$PIDFILE_BACKEND" ] && kill -0 "$(cat $PIDFILE_BACKEND)" 2>/dev/null; then
        echo "Backend:  running (PID $(cat $PIDFILE_BACKEND))"
    else
        echo "Backend:  stopped"
    fi

    if [ -f "$PIDFILE_FRONTEND" ] && kill -0 "$(cat $PIDFILE_FRONTEND)" 2>/dev/null; then
        echo "Frontend: running (PID $(cat $PIDFILE_FRONTEND))"
    else
        echo "Frontend: stopped"
    fi
}

case "$1" in
    start)   start ;;
    stop)    stop ;;
    restart) stop; sleep 1; start ;;
    status)  status ;;
    *)
        echo "Usage: $0 {start|stop|restart|status}"
        exit 1
        ;;
esac
