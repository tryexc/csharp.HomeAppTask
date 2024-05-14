using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeAppTask
{
    class Subject
    {
        // ID
        private string id = "-1";  // field
        public string VPid   // property
        {
            get { return id; }
            set { id = value; }
        }

        //Birthday
        private string bday = "-1";  // field
        public string birthday   // property
        {
            get { return bday; }
            set { bday = value; }
        }

        //Sex
        private string sexv = "-1";  // field
        public string sex   // property
        {
            get { return sexv; }
            set { sexv = value; }
        }

        //Education
        private string edu = "-1";  // field
        public string education   // property
        {
            get { return edu; }
            set { edu = value; }
        }

        //Education
        private string sid = "-1";  // field
        public string sessionID   // property
        {
            get { return sid; }
            set { sid = value; }
        }



    }
}
