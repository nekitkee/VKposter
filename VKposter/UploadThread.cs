using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VKposter
{
    class UploadThread
    {

        public Thread thr;
        Form1 form1;

        

        public Wall wall;
        //PROPERTIES TO ACESS WALL VARIABLE FIELDS FROM THREAD OBJECTS
        public string token {get { return wall.token;}set {wall.token = value;}}
        public string dir { get { return wall.dir; } set { wall.dir = value; } }
        public string wallid { get { return wall.id; } set { wall.id = value; } }
        public string commentText { get { return wall.commentText; } set { wall.commentText = value; } }
        public bool isGroup { get { return wall.isGroup; } set { wall.isGroup = value; } }
        public string name { get { return wall.name; } set { wall.name = value; } }
        public string screenName { get { return wall.screenName; } set { wall.screenName = value; } }
        public bool deleteImage { get { return wall.deleteImage; } set { wall.deleteImage = value; } }




        public UploadThread(Form1 form1 , Wall wall)
        {
            this.form1 = form1;
            
            this.wall = wall;
            thr = new Thread(this.RunThread);
            thr.IsBackground = true;
            thr.Name = wall.id;
            thr.Start();
        }

        bool IsImage(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToLower();
            return (extension == ".png" || extension == ".jpg" || extension == ".gif");
        }


        // Enetring point for thread 
        void RunThread()
        {
            while (true)
            {

                int sleeptime = 60000 * 5;
                string logmsg = wall.id + " " + wall.screenName;
                try
                {
                    
                    List<string> imageList = new List<string>(Directory.GetFiles(wall.dir));
                    imageList.RemoveAll(file => !IsImage(file));
                    if (imageList.Count == 0)
                    {
                        logmsg +=" : NO IMAGES IN FOLDER";
                        const int pauseMin = 5;
                        sleeptime = 60000 * pauseMin;
                    }
                    else
                    {
                   
                            Random rnd = new Random();
                            int index = rnd.Next(imageList.Count);
                            string imagePath = imageList[index];
                        //string response = VkApi.postPhotoOnWall(wall.token, wall.wallid, imagePath, wall.commentText);
                            string response = VkApi.postPhotoOnWall(wall, imagePath);
                            if (wall.deleteImage)
                                File.Delete(imagePath);
                            logmsg += " : " + response;
                            const int pauseMin = 30;
                            int randomDeltaPauseMin = rnd.Next(20);
                            sleeptime = 60000 * (pauseMin + randomDeltaPauseMin);
                   
                    }

                }
                catch (VkApiException ex)
                {
                    logmsg += ex.Message;
                    const int pauseMin = 3;
                    sleeptime = 60000 * pauseMin;

                }
                catch (Exception ex)
                {
                    logmsg += " : ERROR : " + ex.Message;
                }
                form1.Invoke(form1.myAddLogItemDelegate, logmsg);
                Thread.Sleep(sleeptime);
            }
        }
    }
}
