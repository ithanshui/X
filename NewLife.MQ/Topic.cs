﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NewLife.Threading;

namespace NewLife.MessageQueue
{
    /// <summary>主题</summary>
    public class Topic
    {
        #region 属性
        /// <summary>名称</summary>
        public String Name { get; set; }

        /// <summary>订阅者</summary>
        private ConcurrentDictionary<String, Subscriber> Subscribers { get; } = new ConcurrentDictionary<String, Subscriber>();

        /// <summary>消息队列</summary>
        public Queue<Message> Queue { get; } = new Queue<Message>();
        #endregion

        #region 构造函数
        /// <summary>实例化</summary>
        public Topic()
        {
        }
        #endregion

        #region 订阅管理
        /// <summary>订阅主题</summary>
        /// <param name="user">订阅者</param>
        /// <param name="tag">标签。消费者用于在主题队列内部过滤消息</param>
        /// <param name="onMessage">消费消息的回调函数</param>
        /// <returns></returns>
        public Boolean Add(String user, String tag, Func<Message, Boolean> onMessage)
        {
            if (Subscribers.ContainsKey(user)) return false;
            //Subscriber scb = null;
            //if(Subscribers.TryGetValue(user,out scb))
            //{
            //    if (!tag.IsNullOrEmpty()) scb.AddTag(tag);
            //    return true;
            //}

            var scb = new Subscriber(user, tag, onMessage);
            Subscribers[user] = scb;

#if DEBUG
            var msg = new Message
            {
                Sender = user,
                Tag = "Online",
                Content = "上线啦"
            };
            Enqueue(msg);
#endif

            return true;
        }

        /// <summary>取消订阅</summary>
        /// <param name="user">订阅者</param>
        /// <returns></returns>
        public Boolean Remove(String user)
        {
            if (!Subscribers.Remove(user)) return false;

#if DEBUG
            var msg = new Message
            {
                Sender = user,
                Tag = "Offline",
                Content = "下线啦"
            };
            Enqueue(msg);
#endif

            return true;
        }
        #endregion

        #region 进入队列
        /// <summary>进入队列</summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public Boolean Enqueue(Message msg)
        {
            if (Queue.Count > 10000) return false;

            Queue.Enqueue(msg);

            Notify();

            return true;
        }
        #endregion

        #region 推送消息
        /// <summary>推送通知</summary>
        private void Notify()
        {
            // 扫描一次，一旦发送有消息，则调用线程池线程处理
            if (_Timer == null)
                _Timer = new TimerX(Push, null, 0, 5000);
            else
                _Timer.SetNext(-1);
        }

        private TimerX _Timer;

        private void Push(Object state)
        {
            if (Queue.Count == 0) return;
            if (Subscribers.Count == 0) return;

            //Task.Factory.StartNew(async () =>
            //{
            //    while (Queue.Count > 0)
            //    {
            //        // 消息出列
            //        var msg = Queue.Dequeue();
            //        // 向每一个订阅者推送消息
            //        foreach (var ss in Subscribers.Values.ToArray())
            //        {
            //            if (ss.User != msg.Sender)
            //                await ss.NoitfyAsync(msg);
            //        }
            //    }
            //});
        }
        #endregion
    }
}