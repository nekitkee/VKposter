using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VKposter
{
    class VkApiException : Exception
    {
        public VkApiException()
        {

        }

        public VkApiException(string message) : base(" VK Error : "+message)
        {

        }
    }
}
