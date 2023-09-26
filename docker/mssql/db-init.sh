#!/bin/bash

# wait for the SQL Server to come up
sleep 30s

echo "[+] Running SQL Setup Script"

# run the setup script to create the DB and the schema in the DB
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P im938eKAW3K7qM0GZ -d master -i db-init.sql

exit 0