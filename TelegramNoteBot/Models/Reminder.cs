using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramNoteBot
{
    public class Reminder 
    {
        public long userId { get; set; }
        public long reminderId { get; set; }
        public string Text { get; set; }

    }
}
