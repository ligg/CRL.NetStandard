﻿using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CRL.Core.Extension;
using RabbitMQ.Client.Events;

namespace CRL.RabbitMQ
{
    /// <summary>
    /// 主题队列
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TopicRabbitMQ : AbsRabbitMQ
    {
        public TopicRabbitMQ(string host, string user, string pass, string exchangeName, bool consumersAsync = false) : base(host, user, pass,consumersAsync)
        {
            __exchangeName = exchangeName;
            Log($"Topic队列: 初始化");
        }

        public void Publish<T>(string routingKey, params T[] msgs)
        {
            BasePublish(ExchangeType.Topic, routingKey, msgs);
        }
        public void BeginReceive<T>(Action<T, string> onReceive, string routingKey)
        {
            var channel = CreateConsumerChannel();

            var queueName = channel.QueueDeclare().QueueName;
            //绑定队列到topic类型exchange，需指定路由键routingKey
            channel.QueueBind(queueName, __exchangeName, routingKey);
            Log($"开始消费,类型:Topic 队列:{queueName} Key:{routingKey}");
            base.BaseBeginReceive(channel, queueName, onReceive);
        }
    }
}
