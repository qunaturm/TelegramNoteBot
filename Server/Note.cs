using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelegramNoteBot
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public int userId { get; set; }

        //[BsonElement("NoteText")]
        public string Text { get; set;  }
        bool isRemind { get; set; } = false;

    }
}
