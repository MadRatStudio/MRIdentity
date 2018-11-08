using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MRDb.Domain;
using MRDb.Infrastructure.Interface;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Entities.Tasks
{
    public class EmailSendTask : Entity, IEntity
    {
        [BsonRepresentation(BsonType.String)]
        public EmailTaskBot Bot { get; set; }

        [BsonRepresentation(BsonType.String)]
        public EmailSendStatus Status { get; set; } = EmailSendStatus.New;

        public string ToEmail { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string FailMessage { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))] 
    public enum EmailTaskBot
    {
        MadRatBot = 0
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EmailSendStatus
    {
        New = 0,
        Canceled,
        InProgress,
        Delivered,
        Failed
    }
}
