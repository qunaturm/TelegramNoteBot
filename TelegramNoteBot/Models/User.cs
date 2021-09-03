using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNoteBot
{
    public class User
    {
        [BsonId]
        public long Id { get; set; }
        public UserState State { get; set; }
    }
}
