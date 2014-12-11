﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Framework.DesignTimeHost.Models;
using Newtonsoft.Json;

namespace DesignTimeHostDemo
{
    public class ProcessingQueue
    {
        private readonly BinaryReader _reader;
        private readonly BinaryWriter _writer;

        public event Action<Message> OnReceive;

        public ProcessingQueue(Stream stream)
        {
            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);
        }

        public void Start()
        {
            Trace.TraceInformation("[ProcessingQueue]: Start()");
            new Thread(ReceiveMessages) { IsBackground = true }.Start();
        }

        public void Post(Message message)
        {
            lock (_writer)
            {
                Trace.TraceInformation("[ProcessingQueue]: Post({0})", message.MessageType);
                _writer.Write(JsonConvert.SerializeObject(message));
            }
        }

        private void ReceiveMessages()
        {
            while (true)
            {
                try
                {
                    var message = JsonConvert.DeserializeObject<Message>(_reader.ReadString());
                    Trace.TraceInformation("[ProcessingQueue]: OnReceive({0})", message.MessageType);
                    OnReceive(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
