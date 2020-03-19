using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VKposter
{
    public partial class Form1 : Form
    {


        Dictionary<string , UploadThread> wallDict;
        BindingList<string> wallDictKeysCopy;

        string token;
        string dir;
        string wallid;
        string commentText;
        string name;
        bool isGroup;
        bool deleteImage;
        string screenName;


        public delegate void AddLogItem(string item);
        public AddLogItem myAddLogItemDelegate;



      


        public Form1()
        {
            InitializeComponent();
            wallDict = new Dictionary<string, UploadThread>();
            wallDictKeysCopy = new BindingList<string>();
            listBoxRunning.DataSource = wallDictKeysCopy;
            myAddLogItemDelegate = new AddLogItem(makeLog);
            this.Height = 650;
            this.Width = 920;
            this.Show();

            loadSavedInfo();

        }

        string storageFile = "VKposterDATA";

        void loadSavedInfo()
        {
            string path = System.Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).ToString();
            storageFile = $"{path}/{storageFile}";
            //load json from file
            if (File.Exists(storageFile))
            {
                using (StreamReader file = File.OpenText(storageFile))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    List < Wall> walls = (List <Wall>)serializer.Deserialize(file, typeof(List<Wall>));
                    foreach (Wall wall in walls)
                    {
                        //start thread 
                        //add to dict 
                        //add to BindingList
                        addThread(wall);
                    }
                }
            }
            else
            {
                makeLog("No saved data to load...");
            }
        }


        void makeLog(string message)
        {
            message = "[ " + DateTime.Now.ToString("HH:mm:ss tt") + "] " + message;

            //add to log window
            this.listBoxLogs.Items.Add(message);
            //append to log file
            Logger.ToFile(message);
        }

        string Win1251ToUtf8(string input)
        {
            Encoding utf8 = Encoding.GetEncoding("UTF-8");
            Encoding win1251 = Encoding.GetEncoding("Windows-1251");

            byte[] utf8Bytes = win1251.GetBytes(input);
            byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);

            return win1251.GetString(win1251Bytes);
        }

        private void buttonRun_Click(object sender, EventArgs e)
        {

            try
            {

                wallid = textBoxGroupId.Text;
                token = textBoxToken.Text;
                commentText = richTextBoxComment.Text;
                dir = textBoxFolder.Text;
                isGroup = checkBoxIsGroup.Checked;
                deleteImage = checkBoxDelete.Checked;

                //ADD NAME AND SCREEN NAME
                if (isGroup)
                {

                    var tuple = VkApi.getById(token, wallid);
                    screenName = tuple.Item1;
                    // name = tuple.Item2;
                    name = Win1251ToUtf8(tuple.Item2);
                    wallid = "-" + tuple.Item3;
                }
                else
                {
                    name = screenName = wallid;
                }


                //ADD TO DICT OR UPDATE THREAD
                if (wallDictKeysCopy.Contains(screenName))
                {
                    wallDict[screenName].dir = dir;
                    wallDict[screenName].commentText = commentText;
                    wallDict[screenName].token = token;
                    wallDict[screenName].isGroup = isGroup;
                    wallDict[screenName].deleteImage = deleteImage;

                    makeLog(screenName + " settings updated");

                }
                else
                {
                    Wall wall = new Wall(token, dir, wallid, commentText, isGroup, name, screenName , deleteImage);

                    UploadThread currentThread = new UploadThread(this, wall);
                    wallDict.Add(screenName, currentThread);
                    wallDictKeysCopy.Add(screenName);
                    makeLog(screenName + " NEW thread started");
                    int index = listBoxRunning.FindString(screenName);
                    listBoxRunning.SetSelected(index, true);

                }
            }

            catch (VkApiException ex)
            {
                makeLog(ex.Message);

            }
            catch (Exception ex)
            {
                makeLog(ex.Message);
            }
        }


        void addThread(Wall wall)
        {
            try
            {

                UploadThread currentThread = new UploadThread(this, wall);
                wallDict.Add(wall.screenName, currentThread);
                wallDictKeysCopy.Add(wall.screenName);
                makeLog(wall.screenName + " NEW thread started");
                int index = listBoxRunning.FindString(wall.screenName);
                listBoxRunning.SetSelected(index, true);
            }
            catch (VkApiException ex)
            {
                makeLog(ex.Message);

            }
            catch (Exception ex)
            {
                makeLog(ex.Message);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {

            if (wallDictKeysCopy.Contains(screenName))
            {
                wallDict[screenName].thr.Abort();
                wallDict.Remove(screenName);
                wallDictKeysCopy.Remove(screenName);
             
            }
           
        }

        private void listBoxRunning_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxRunning.SelectedItem != null)
            {
                string selectedWall = listBoxRunning.SelectedItem.ToString();
                int index = listBoxRunning.FindString(selectedWall);
                if (index != -1)
                {
                    screenName  = textBoxGroupId.Text = wallDict[selectedWall].screenName;
                    wallid = wallDict[selectedWall].wallid;
                    //textBoxGroupId.Text = (wallid.StartsWith("-")) ? wallid.Substring(1) : wallid;
                    token = textBoxToken.Text = wallDict[selectedWall].token;
                    commentText = richTextBoxComment.Text = wallDict[selectedWall].commentText;
                    dir = textBoxFolder.Text = wallDict[selectedWall].dir;
                    name = labelName.Text = wallDict[selectedWall].name;
                    deleteImage = checkBoxDelete.Checked = wallDict[selectedWall].deleteImage;
                    isGroup = checkBoxIsGroup.Checked = wallDict[selectedWall].isGroup;

                }
            }
        }

        private void buttonChooseFolder_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                textBoxFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            List<Wall> walls = new List<Wall>();
            foreach(string key in wallDictKeysCopy)
            {
                walls.Add(wallDict[key].wall);
            }

            File.WriteAllText(storageFile, JsonConvert.SerializeObject(walls));
            makeLog("Running threads saved to storage");
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string link = "https://github.com/nekitkee/VKposter/wiki/VK-poster-v1.0";
            makeLog("Opening manual...");
            System.Diagnostics.Process.Start(link);
        }
    }
}
