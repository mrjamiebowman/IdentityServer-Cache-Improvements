#!/bin/bash
# start SQL Server, start the script to create/setup the DB
# You need a non-terminating process to keep the container alive. 
# In a series of commands separated by single ampersands the commands to the left of the right-most ampersand are run in the background. 
# So - if you are executing a series of commands simultaneously using single ampersands, the command at the right-most position needs to be non-terminating
/db-init.sh & /opt/mssql/bin/sqlservr

echo "[+] Database Setup Complete"
exit 0

# # Start SQL Server
# /opt/mssql/bin/sqlservr &

# # Initialize the database
# # Sleep time to wait until SQL Server is ready
# sleep 30s

# # Run the SQL script
# /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "GXQZdhNCmFfW7kC0" -d master -i /db-init.sql

# # Keep the container running
# tail -f /dev/null