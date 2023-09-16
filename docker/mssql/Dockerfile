FROM mcr.microsoft.com/mssql/server:2019-latest
EXPOSE 1433

ENV MSSQL_PID=Developer
ENV MSSQL_AGENT_ENABLED=True 

COPY . /

# use root user for configuration
USER root

# config db
RUN chmod +x /entrypoint.sh
RUN chmod +x /db-init.sh

# # USER mssql
CMD ["/bin/bash", "/entrypoint.sh"]
