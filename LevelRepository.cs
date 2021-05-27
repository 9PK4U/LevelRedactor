using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LevelRedactor.Parser.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace LevelRedactor
{
    public class LevelRepository
    {
        private ObservableCollection<LevelData> levelsData;
        private IMongoDatabase database;

        public LevelRepository()
        {
            levelsData = new();

            MongoClientSettings settings = new();
            settings.Server = new MongoServerAddress("176.99.11.108", 27017);
            MongoClient client = new(settings);
            database = client.GetDatabase("GameData");
        }

        public void SendSetToServer(string jsonDocumentStr)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Maps");
            IMongoCollection<BsonDocument> collectionUpdate = database.GetCollection<BsonDocument>("Configurations");

            collectionUpdate.UpdateOneAsync(
                new BsonDocument("Update", "Levels"),
                new BsonDocument("$set", new BsonDocument("DataLevelUpdate", DateTime.Now.ToString())));


            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonDocumentStr);

            collection.InsertOne(bsonDocument);
            
        }
    }
}
