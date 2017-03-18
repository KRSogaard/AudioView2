using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AudioView.Web.Tools
{

    public enum FlashType
    {
        Success,
        Notice,
        Error
    };

    public class FlashMessenger
    {

        private Dictionary<FlashType, Queue<string>> _messages;
        public Dictionary<FlashType, Queue<string>> Messages
        {
            get { return _messages; }
            private set { _messages = value; }
        }

        public FlashMessenger()
        {
            Messages = new Dictionary<FlashType, Queue<string>>();
            foreach (FlashType type in Enum.GetValues(typeof(FlashType)))
            {
                Messages[type] = new Queue<string>();
            }
        }
    }

    public class FlashHelper
    {
        public static FlashMessenger Messenger
        {
            get
            {
                if (HttpContext.Current.Session["FlashMessenger"] == null)
                    HttpContext.Current.Session["FlashMessenger"] = new FlashMessenger();
                return (FlashMessenger)HttpContext.Current.Session["FlashMessenger"];
            }
            private set
            {
                HttpContext.Current.Session["FlashMessenger"] = value;
            }
        }

        public static void Add(string message)
        {
            Add(message, FlashType.Success);
        }

        public static void Add(string message, FlashType type)
        {
            Messenger.Messages[type].Enqueue(message);
        }

        public static string[] Get(FlashType type)
        {
            var array = Messenger.Messages[type].ToArray();
            Messenger.Messages[type].Clear();
            return array;
        }

        public static int Count(FlashType type)
        {
            return Messenger.Messages[type].Count;
        }

        public static void Clear()
        {
        }
    }
}