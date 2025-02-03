import config
import esdbclient
import logging
import ssl
from esdbclient.connection_spec import ConnectionSpec

log = logging.getLogger(__name__)

def create_db_client() -> esdbclient.EventStoreDBClient:
    log.info(f"Connecting to the database: url={config.ESDB_URL}")
    spec = ConnectionSpec(config.ESDB_URL)
    server_certificate = None
    if spec.options.Tls:
        target = spec.targets[0]
        target_coordinates = target.split(":")
        log.info(f"Database TLS is enabled, fetching certificate details: host={target_coordinates[0]}")
        server_certificate = ssl.get_server_certificate(addr=(target_coordinates[0], int(target_coordinates[1])))
        log.info(f"Database TLS is enabled, fetching certificate details...done.")
    return esdbclient.EventStoreDBClient(uri=config.ESDB_URL, root_certificates=server_certificate)