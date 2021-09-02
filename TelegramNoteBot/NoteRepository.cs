using MongoDB.Bson;
using MongoDB.Driver;

namespace TelegramNoteBot
{
    public class NoteRepository
    {
        private IMongoCollection<Note> _notesCollection;
        public NoteRepository(IMongoCollection<Note> notesCollection)
        {
            _notesCollection = notesCollection;
        }

        public void AddNewNote(Note note)
        {
            _notesCollection.InsertOne(note);
        }

        public void GetAllNotes(long userId)
        {
            foreach(var doc in _notesCollection)
            {
                if (n)
            }
            //var userNotes = _notesCollection.Find(.Equals(userId), );
        }
    }
}
