﻿using PlexShareNetwork;
using PlexShareWhiteboard.BoardComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlexShareNetwork;
using PlexShareWhiteboard.Server.Interfaces;
using PlexShareNetwork.Communication;
using PlexShareNetwork.Serialization;
using System.Diagnostics;
using PlexShareWhiteboard.Client;
using Serializer = PlexShareWhiteboard.BoardComponents.Serializer;

namespace PlexShareWhiteboard.Server
{
    public class ServerCommunicator: IServerCommunicator
    {
        private static ServerCommunicator instance;
        private static Serializer serializer;
        private static ICommunicator communicator;
        private WhiteBoardViewModel _vm;
        private static readonly string moduleIdentifier = "Whiteboard";

        public static ServerCommunicator Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ServerCommunicator();
                    serializer = new Serializer();
                    communicator = CommunicationFactory.GetCommunicator(false);
                    communicator.Subscribe(moduleIdentifier, WhiteBoardViewModel.Instance);
                }

                return instance;
            }
        }

        //public void SetVMRef(WhiteBoardViewModel vm)
        //{
        //    _vm = vm;
        //    communicator.Subscribe(moduleIdentifier, _vm);
        //}

        public void Broadcast(ShapeItem newShape, Operation op)
        {
            List<ShapeItem> newShapeList = new List<ShapeItem>();
            newShapeList.Add(newShape);
            Broadcast(newShapeList, op);
        }

        public void Broadcast(List<ShapeItem> newShapes, Operation op)
        {
            List<SerializableShapeItem> newSerializableShapes = serializer.ConvertToSerializableShapeItem(newShapes);
            WBServerShape clientUpdate = new WBServerShape(newSerializableShapes, op);
            Broadcast(clientUpdate);
        }

        public void Broadcast(WBServerShape clientUpdate, string? ipAddress=null)
        {
            try
            {
                Trace.WriteLine("[Whiteboard] ServerCommunicator.Broadcast: Sending objects to client");
                var serializedObj = serializer.SerializeWBServerShape(clientUpdate);
                communicator.Send(serializedObj, moduleIdentifier, ipAddress);
                Trace.WriteLine("[Whiteboard] ServerCommunicator.Broadcast: Sent objects to client");
            }
            catch (Exception e)
            {
                Trace.WriteLine("[Whiteboard] ServerCommunicator.Broadcast: Exception Occured");
                Trace.WriteLine(e.Message);
            }
        }
    }
}
