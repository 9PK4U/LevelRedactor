using System;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;
using MongoDB.Bson.Serialization;
using LevelRedactor.Parser.Models;
using System.Collections.ObjectModel;

namespace LevelRedactor
{
    public class LevelRepository
    {
        private IMongoDatabase database;

        public LevelRepository()
        {
            MongoClientSettings settings = new();
            settings.Server = new MongoServerAddress("176.99.11.108", 27017);
            MongoClient client = new(settings);
            database = client.GetDatabase("GameData");
        }

        public void SendSetToServer(LevelData levelData)
        {
            IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Maps");
            IMongoCollection<BsonDocument> collectionUpdate = database.GetCollection<BsonDocument>("Configurations");

            var updateFilter = Builders<BsonDocument>.Filter.Eq("Update", "Levels");

            var updateBson = Builders<BsonDocument>.Update.Set("DataLevelUpdate", DateTime.Now.ToString());

            var updateResult = collectionUpdate.UpdateOne(updateFilter, updateBson);

            if (updateResult.MatchedCount == 0)
            {
                var updateBSON = new BsonDocument("DataLevelUpdate", DateTime.Now.ToString())
                {
                    new BsonDocument("Update", "Levels")
                };
                collectionUpdate.InsertOne(updateBSON);
            }

            string jsonData = JsonSerializer.Serialize(levelData);
            var bsonDocument = BsonSerializer.Deserialize<BsonDocument>(jsonData);

            var filter = new BsonDocument("$and", new BsonArray
            {
                new BsonDocument("Title", levelData.Title),
                new BsonDocument("Tag", levelData.Tag)
            });

            if (collection.ReplaceOne(filter, bsonDocument).IsAcknowledged)
                collection.InsertOne(bsonDocument);
        }
    }
}

