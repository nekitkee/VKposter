using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKposter
{
    class Wall
    {
        public string screenName;
        public string name;
        public bool isGroup;
        public string token;
        public string dir;
        public string id;
        public string commentText;
        public bool deleteImage;

        public Wall(string token, string dir, string wallid, string commentText,
            bool isGroup, string name, string screenName , bool deleteImage)
        {
            this.token = token;
            this.dir = dir;
            this.id = wallid;
            this.commentText = commentText;
            this.isGroup = isGroup;
            this.name = name;
            this.screenName = screenName;
            this.deleteImage = deleteImage;
        }

    }
}
