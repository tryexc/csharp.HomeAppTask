using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.IO;
using System.Drawing.Drawing2D;

namespace HomeAppTask
{
    public partial class Form1 : Form, IMessageFilter
    {
        delegate void LabelDelegate(string message);
        private readonly String[] inputParameterNames = {"stimDuration",
                                                "rampUpTime",
                                                "rampDownTime",
                                                "isResume",
                                                "preStimDuration",
                                                "postStimDuration",
                                                "port",
                                                "screenSize"};

        private int SelBox = -1;

        private bool cancel;
        
        private bool regularClose = false;

        private Label[] SelLabel = new Label[4];

        private Panel[] SelBoxPanel = new Panel[4];
        
        private string rootDir = "";

        private XmlDocument TrainingList = null;
        private XmlDocument Subject = null;
        private XmlDocument Initial = null;

    
        private String[,] TrainingArray;

        private Subject sessionSubject = null;
        private InputParameter sessionParameter = null;


        private readonly String[] Education = { "Hauptschule", "mittlere Reife", "Abitur", "Universitaet" };
        private readonly String[] Sex = { "female", "male" };

        private string LogPrefix = "";

        private string LogSuffix = "";

        private bool NextClick = false;
        private int NumOfRetr = 0;

        int numOfMainLists;
        //DateTime startTime;
        DialogResult CloseDialogRes;


        /// <summary>
        /// Store session parameter in an new object 
        /// </summary>
        /// <param name="args"></param>
        private void StoreInputParameter(String[] args)
        { 
            foreach(string item in inputParameterNames)
            {   
                if(args.Contains("-" + item))
                {
                    try
                    {
                        int argIdx = Array.IndexOf(args, "--" + item);
                        int argValue = Convert.ToInt32(args[argIdx + 1]);
                        Type thisType = typeof(InputParameter);
                        PropertyInfo pi = thisType.GetProperty(item);
                        pi.SetValue(sessionParameter, argValue, null);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString());
                    }
                }
 
            }
        }
       
        public Form1(string[] args)
        {
            Application.AddMessageFilter(this);
            InitializeComponent();
            this.FormClosing += Form1_FormClosing;
            sessionParameter = new InputParameter();

            if (args != null)
            {
                StoreInputParameter(args);

                //MessageBox.Show(sessionParameter.port.ToString());
                //MessageBox.Show(sessionParameter.screenSize.ToString());

            }
            //Starts the "heart-beat" -> via TCP/IP to say "i´m alive"!!
            HeartBeat hb = new HeartBeat();
            Task heartbeat = Task.Run(() => hb.HeartBeat_start(sessionParameter.port));
           
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!regularClose)
            {
                
                //switch (MessageBox.Show(this, "Möchten Sie die Aufgabe wirklich beenden?", "Abbruch?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                switch (CloseDialogRes)
                {
                    //Stay on this form
                    case DialogResult.No:
                        e.Cancel = true;
                        cancel = false;
                        break;
                    default:
                        break;
                }
                
            }
            
         }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool PreFilterMessage(ref Message m)
        {
            //Wird durchlaufen, wenn Winows eine Meldung an das Fenster schickt
            //Die Windows-Meldung ist das KeyDown-Event
            if (m.Msg == 0x100)
            {
                switch ((Keys)m.WParam | (Keys)Control.ModifierKeys)//Der WParam-Parameter enthält die gedrückte Taste, Control.ModifizierKeys entspricht den gedrückten Tasten wie Strg, Shift usw.
                                                                    //Mit dem | werden diese zu einem Wert "verbunden"
                {
                    case Keys.F1://Auch die Konstanten Vergleichswerte kann man verbinden und somit vergleichen
                        MessageBox.Show("ID: " + sessionSubject.VPid +
                            "\nBirthday: " + sessionSubject.birthday +
                            "\nSEX: " + sessionSubject.sex +
                            "\nEducation: " + sessionSubject.education +
                            "\nSession: " + TrainingArray[Convert.ToInt32(sessionSubject.sessionID),0],
                            "Subject - Info!");
                        break;
                    case Keys.F2:
                        MessageBox.Show("Inter-Stimulus-Interval: "+ sessionParameter.ISI.ToString() + 
                                        "\nPic-Presentation-Time: " + sessionParameter.PPT.ToString() +
                                        "\nPre-Stim-Duration: " + sessionParameter.preStimDuration.ToString() +
                                        "\nRamp-Up-Time: " + sessionParameter.rampUpTime.ToString() +
                                        "\nStim-Duration: " + sessionParameter.stimDuration.ToString() +
                                        "\nRamp-Down-Time: " + sessionParameter.rampDownTime.ToString() +
                                        "\nPost-Stim-Duration: " + sessionParameter.postStimDuration.ToString() +
                                        "\nScreen-Size: " + sessionParameter.screenSize +
                                        "\nPort: " + sessionParameter.port, "Session - Info!");
                        break;
                    case Keys.Escape:
                        cancel = true;
                        abort();
                        break;
                        //usw.
                }
            }
            return false;//Meldung wurde nicht verarbeitet, also false zurück geben - für eine verarbeitete Tastenkombination kann man auch true zurück geben

        }



        private void Form1_Load(object sender, EventArgs e)
        {

          
            cancel = false;
          
            // Tab
            tabControl1.Appearance = TabAppearance.FlatButtons;
            tabControl1.ItemSize = new Size(0, 1);
            tabControl1.SizeMode = TabSizeMode.Fixed;

            //
            panel7.BackColor = Color.Beige;

            // ExePath
            rootDir = Application.StartupPath;

            //// Button Style
            //GraphicsPath p = new GraphicsPath();
            //p.AddEllipse(1, 1, button2.Width-4, button2.Height -4);
            //button2.Region = new Region(p);

            // Sel Labels
            SelLabel[0] = labelLetter1;
            SelLabel[1] = labelLetter2;
            SelLabel[2] = labelLetter3;
            SelLabel[3] = labelLetter4;
            // Sel Box Panel
            SelBoxPanel[0] = panel1;
            SelBoxPanel[1] = panel2;
            SelBoxPanel[2] = panel3;
            SelBoxPanel[3] = panel4;

            // set path to initial xml, and load it
            Initial = new XmlDocument();
            Initial.Load(rootDir + "\\" + "initial.xml");
            loadInitialItems();

            //set path to training list (xml), and load ist
            TrainingList = new XmlDocument();
            TrainingList.Load(rootDir + "\\" + "lists.xml");
            loadTrainingItems();

            // set path to single subject xml, and load it
            sessionSubject = new Subject();
            Subject = new XmlDocument();
            Subject.Load(rootDir + "\\" + "subject.xml");
            loadSubjectItems();

            


        }

        /// <summary>
        /// Load Training Items from xml
        /// </summary>
        private void loadTrainingItems()
        {
            XmlElement root = TrainingList.DocumentElement;
            numOfMainLists = root.ChildNodes.Count;

            TrainingArray = new String[numOfMainLists, 3];

            int n = 0;
            foreach (XmlNode node in TrainingList.DocumentElement )
            {
                string name = node.Attributes[0].InnerText;
               
                string numOfSubLists = node.Attributes[2].InnerText;

                TrainingArray[n,0] = name;
                TrainingArray[n,1] = node.InnerText;
                TrainingArray[n,2] = numOfSubLists;
                n += 1;

               //  MessageBox.Show(numOfLists);
            }

        }


        /// <summary>
        /// Load Subject Items
        /// </summary>
        private void loadSubjectItems()
        {
            XmlElement nodes = Subject.DocumentElement;
            sessionSubject.VPid = nodes["id"].InnerText;
            sessionSubject.birthday = nodes["birthday"].InnerText;
            sessionSubject.sex = nodes["sex"].InnerText;
            sessionSubject.education = nodes["education"].InnerText;
            sessionSubject.sessionID = nodes["sessionID"].InnerText;


            LogPrefix = sessionSubject.VPid + "\t" + Array.IndexOf(Sex, sessionSubject.sex) + "\t" + sessionSubject.sex +
                        "\t" + Array.IndexOf(Education, sessionSubject.education) + "\t" + sessionSubject.education + "\t" + sessionSubject.birthday +
                        "\t" + "LETTUP" + "\t" + sessionSubject.sessionID + "\t" + "1" + "\t" + "PRACT" + "\t" + "1" + "\t" + sessionParameter.PPT.ToString() +
                        "\t" + sessionParameter.ISI.ToString(); 

        }

        private void loadInitialItems()
        {
            XmlElement nodes = Initial.DocumentElement;
            sessionParameter.PPT = Convert.ToInt32(nodes["ppt"].InnerText);
            sessionParameter.ISI = Convert.ToInt32(nodes["isi"].InnerText);

        }

         private void Test(String[,] StimList, bool practice)
        {
            var startDate = DateTime.Now;
            string dt = startDate.Year.ToString() + startDate.Month.ToString() + startDate.Day.ToString() + startDate.Hour.ToString() + startDate.Minute.ToString() + startDate.Second.ToString();
            int sid = -1;

            if (practice){sid = 0;}
            else{sid = Convert.ToInt32(sessionSubject.sessionID);}
            
            string SessionName = StimList[sid, 0];
            string SessionStr = StimList[sid, 1];
            string SessionNumofLists = StimList[sid, 2];
           
            string[] LetterList = SessionStr.Split('$');

            int corrNum = 0;
            for (int i = 0; i < LetterList.Length; i++ )
            {
                LogSuffix = "";
                if (cancel) { break; }
                
                string[] Last4 = EncSingleRun(LetterList[i]);

                
                int retrNum = Retr(Last4);

                if (retrNum == 4)
                {
                    corrNum += 1;
                }

                var endDate = System.DateTime.Now;
                TimeSpan diff = endDate.Subtract(startDate);

                LogSuffix += "\t" + Math.Round(diff.TotalMilliseconds,0).ToString();

                if (!cancel)
                {
                    //MessageBox.Show(retrNum.ToString());
                    using (StreamWriter outputFile = new StreamWriter(rootDir + "\\results\\"  + dt +"_" + sessionSubject.VPid + "_" + SessionName + ".txt", true))
                    {
                        outputFile.WriteLine(LogPrefix + "\t" + LogSuffix);

                        outputFile.Close();
                    }


                }
                
            }

            //write sessionID to subject xml file
            if(Convert.ToInt32(sessionSubject.sessionID) <= numOfMainLists && !cancel && !practice)
            {
                XmlElement nodes = Subject.DocumentElement;
                nodes["sessionID"].InnerText = Convert.ToString(Convert.ToInt32(sessionSubject.sessionID) + 1);
                Subject.Save(@rootDir + "\\" + "subject.xml");
            }

            
            
            if(practice)
            {
                button5.Visible = false;
                Msg m = new Msg("Sehr gut!\n\nDer Testdurchlauf ist nun beendet.");
                m.ShowDialog();
                tabControl1.SelectedIndex = 1;

            }
            else
            {
                //feedback to subject and goodbye
                label15.Text = "Vielen Dank, die Aufgabe ist nun beendet!\nSie haben von insgesamt " + LetterList.Length + " Listen, " + corrNum + " korrekt wiedergegeben!";
                
                tabControl1.SelectedIndex = 5;
            }
            


        }
        private string[] EncSingleRun(String SessionStr)
        {

            tabControl1.SelectedIndex = 2;


            string[] singleLetter = SessionStr.Split('-');


            for (int i = 1; i < singleLetter.Length; i++)
            {

                if (cancel) { break; }
                LogSuffix += singleLetter[i];

                var endDate = DateTime.Now + TimeSpan.FromMilliseconds(sessionParameter.ISI);
                while (DateTime.Now < endDate && !cancel)
                {
                    Application.DoEvents();
                }
                label4.Text = singleLetter[i];
                label4.Refresh();
              
                endDate = DateTime.Now + TimeSpan.FromMilliseconds(sessionParameter.PPT);
                while (DateTime.Now < endDate && !cancel)
                {
                    Application.DoEvents();
                }
                label4.Text = "";
                label4.Refresh();
                
            }
            string[] l4 = { singleLetter[singleLetter.Length-4],
                            singleLetter[singleLetter.Length-3],
                            singleLetter[singleLetter.Length-2],
                            singleLetter[singleLetter.Length-1]};
            return l4;

        }

        

        private int Retr(string[] l4)
        {
            resetRetrStyle();

            NumOfRetr++;
            tabControl1.SelectedIndex = 3;
            NextClick = false;

            int retrRAC = 0;

            while (SelLabel[0].Text =="X" || SelLabel[1].Text == "X" || SelLabel[2].Text == "X" || SelLabel[3].Text == "X")
            {
                if (cancel) { break; }
                Application.DoEvents();
            }

            while (!NextClick)
            {
                if (cancel) { break; }
                Application.DoEvents();
            }

            LogSuffix += "\t" + NumOfRetr.ToString();
           

            for (int i = 0; i < SelLabel.Length; i++)
            {
                LogSuffix += "\t" + l4[i];
                LogSuffix += "\t" + SelLabel[i].Text;

                if(l4[i] == SelLabel[i].Text)
                {
                    LogSuffix += "\t1";
                    retrRAC += 1; // correct reaction, if =4 the all items right

                }
                else
                {
                    LogSuffix += "\t0";
                }


            }
            return retrRAC;

            //MessageBox.Show(LogPrefix + "\n" + LogSuffix);

        }

        private void resetRetrStyle()
        {
            SelBox = -1;
            button5.Visible = false;
                        
            for(int i = 0; i<4;i++)
            {
                SelLabel[i].Text = "X";
                SelLabel[i].Visible = false;
                SelLabel[i].Refresh();
                SelBoxPanel[i].BackColor = default(Color);

            }
            


        }

        private void abort()
        {
            cancel = true;
            MsgClose clsDlg = new MsgClose();
            CloseDialogRes = clsDlg.ShowDialog();
            clsDlg.Dispose();
            Program.ExitApplication(1);

        }


        private void button1_Click(object sender, EventArgs e)
        {
            abort();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(Convert.ToInt32(sessionSubject.sessionID) < numOfMainLists)
            {

                Test(TrainingArray,false);
            }
            else
            {
               // MessageBox.Show(this, "Sie haben bereits alle Sitzungen absolviert.\nDas Programm wird beendet.\nKlicken sie auf OK!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Msg m = new Msg("Sie haben bereits alle Sitzungen absolviert.\nDas Programm wird beendet.\nKlicken sie auf Weiter!");

                m.ShowDialog();
                
                regularClose = true;
                Program.ExitApplication(0);

            }
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
        }

       private bool CheckIfAllselected()
       {
            for (int i = 0; i < SelLabel.Length; i++)
            {
                if (String.Equals(SelLabel[i].Text, "X"))
                {
                    return false;

                }

            }
            return true;

        }


        /// <summary>
        /// Change BgColor of selected box
        /// </summary>
        /// <param name="selbox"></param>
        private void ColorPanels(int selbox)
        {
            for (int i = 0; i < SelBoxPanel.Length; i++)
            {
                SelBoxPanel[i].BackColor = default(Color);


            }
            SelBoxPanel[selbox].BackColor = Color.Beige;

        }

        /// <summary>
        /// Box-1
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseClick(Object sender, MouseEventArgs e)
        {           
            SelBox = 0;
            ColorPanels(SelBox);
        }
        private void labelLetter1_Click(object sender, EventArgs e)
        {
            SelBox = 0;
            ColorPanels(SelBox);

        }

        /// <summary>
        /// Box-2
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel2_MouseClick(Object sender, MouseEventArgs e)
        {
            SelBox = 1;
            ColorPanels(SelBox);
        }
        private void labelLetter2_Click(object sender, EventArgs e)
        {
            SelBox = 1;
            ColorPanels(SelBox);
        }

        /// <summary>
        /// Box-3
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel3_MouseClick(Object sender, MouseEventArgs e)
        {
            SelBox = 2;
            ColorPanels(SelBox);
             
        }
        private void labelLetter3_Click(object sender, EventArgs e)
        {
            SelBox = 2;
            ColorPanels(SelBox);

        }

        /// <summary>
        /// Box-4
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel4_MouseClick(Object sender, MouseEventArgs e)
        {
            SelBox = 3;
            ColorPanels(SelBox);
        }
        private void labelLetter4_Click(object sender, EventArgs e)
        {
            SelBox = 3;
            ColorPanels(SelBox);

        }

        /// <summary>
        /// A
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label5_Click(object sender, EventArgs e)
        {
            if (SelBox != -1)
            {
                SelLabel[SelBox].Visible = true;
                SelLabel[SelBox].Text = label5.Text ;

                if (CheckIfAllselected())
                {
                    button5.Visible = true;

                }
            }
        }

        /// <summary>
        /// B
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label6_Click(object sender, EventArgs e)
        {
            if (SelBox != -1)
            {
                SelLabel[SelBox].Visible = true;
                SelLabel[SelBox].Text = label6.Text;

                if (CheckIfAllselected())
                {
                    button5.Visible = true;

                }
            }

        }

        /// <summary>
        /// C
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label7_Click(object sender, EventArgs e)
        {
            if (SelBox != -1)
            {
                SelLabel[SelBox].Visible = true;
                SelLabel[SelBox].Text = label7.Text;

                if (CheckIfAllselected())
                {
                    button5.Visible = true;

                }
            }

        }

        /// <summary>
        /// D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void label8_Click(object sender, EventArgs e)
        {
            if (SelBox != -1)
            {
                SelLabel[SelBox].Visible = true;
                SelLabel[SelBox].Text = label8.Text;

                if (CheckIfAllselected())
                {
                    button5.Visible = true;

                }
            }

        }

        private void button5_Click(object sender, EventArgs e)
        {
            NextClick = true;
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            regularClose = true;
            Program.ExitApplication(0);

        }

        private void button6_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Test(TrainingArray, true);
        }
    }
}
