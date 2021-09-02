using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TelegramNoteBot
{
    public class Note
    {
        [BsonId]
        public string Id { get { return $"{UserId}:{NoteId}"; } }
        public long UserId { get; set; }
        public long NoteId { get; set; }

        //[BsonElement("NoteText")]
        public string Text { get; set; }
        public bool IsRemind { get; set; } = false;
        public Note(long userId, long noteId, string Text, bool isRemind)
        {
            this.UserId = userId;
            this.NoteId = noteId;
            this.Text = Text;
            this.IsRemind = isRemind;
        }

    }
}
