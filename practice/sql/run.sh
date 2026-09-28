#!/bin/bash
# Usage: ./practice/sql/run.sh practice/sql/<file>.sql
# Copies the .sql file into the SQL Server container and runs it with sqlcmd.
docker cp "$1" fieldops-sql:/tmp/q.sql && \
docker exec fieldops-sql /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SA_PASSWORD" -C -W -i /tmp/q.sql
