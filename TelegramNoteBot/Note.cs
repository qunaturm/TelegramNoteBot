using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelegramNoteBot
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public long userId { get; set; }

        //[BsonElement("NoteText")]
        public string Text { get; set; }
        public bool isRemind { get; set; } = false;
        public Note(long userId, string Text, bool isRemind)
        {
            this.userId = userId;
            this.Text = Text;
            this.isRemind = isRemind;
        }

    }
}
