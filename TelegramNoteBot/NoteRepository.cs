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

        public /*Task<List<Note>>*/ IFindFluent<Note, Note> GetAllNotes(long userId)
        //public FilterDefinition<Note> GetAllNotes(long userId)
        {
            //var filter = Builders<Note>.Filter.Where(x => x.UserId == userId);
            var doc = _notesCollection.Find(x => "UserId" == userId.ToString());
            return doc;
        }
    }
}
