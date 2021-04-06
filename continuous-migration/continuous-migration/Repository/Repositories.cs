using continuous_migration.Model;
using Microsoft.Azure.Cosmos;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace continuous_migration.Repository
{
    public class Repositories
    {
        public class StagingRepository : DocumentDBRepository<Staging>
        {
            public StagingRepository(CosmosClient client, string databaseName) :
                base(databaseId: databaseName, client, createDatabaseIfNotExist: false)
            { }
        }

        public class ActivityRepository : DocumentDBRepository<Activity>
        {
            public ActivityRepository(CosmosClient client, string databaseName) :
                base(databaseId: databaseName, client, createDatabaseIfNotExist: false)
            { }
        }

        public class EvhConnectorRepository : DocumentDBRepository<EvhConnector>
        {
            public EvhConnectorRepository(CosmosClient client, string databaseName) :
                base(databaseId: databaseName, client, createDatabaseIfNotExist: false)
            { }
        }
    }
}
