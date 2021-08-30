using MongoDB.Bson;
using MongoDB.Driver;

namespace TelegramNoteBot
{
    public class NoteRepository
    {
        private IMongoCollection<BsonDocument> _notesCollection;
        public NoteRepository(IMongoCollection<BsonDocument> notesCollection)
        {
            _notesCollection = notesCollection;
        }

        public void AddNewNote(Note note)
        {
            BsonDocument doc = new BsonDocument
            {
                    {"userId",  note.userId},
                    {"noteId", note.noteId },
                    { "Text", note.Text},
                    { "isRemind", false}
            };

        }
    }
}
